using System;
using CrimeLife.Crime;
using CrimeLife.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CrimeLife.Build
{
    // 심즈식 건축 모드: 카탈로그에서 고르면 고스트가 따라다니고, 클릭으로 놓는다.
    // 기존 가구를 클릭하면 집어서 옮기고, 우클릭하면 판다.
    public class BuildModeController : MonoBehaviour
    {
        private enum Mode { Off, Idle, Placing }

        [SerializeField] private HomeLot _lot;
        [SerializeField] private BuildConfig _config;
        [SerializeField] private Wallet _wallet;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _player;
        [Tooltip("UI 위에서 한 클릭을 가려내는 데 쓴다")]
        [SerializeField] private UIDocument _uiDocument;
        [Tooltip("건축 모드 중에는 꺼 두는 컴포넌트 (클릭 이동 등)")]
        [SerializeField] private Behaviour[] _disableWhileBuilding;
        [SerializeField] private Material _validGhost;
        [SerializeField] private Material _invalidGhost;

        private Mode _mode;
        private FurnitureDefinition _placing;
        private int _rotation;
        private bool _isMove;
        private Vector2Int _moveOriginCell;
        private int _moveOriginRotation;

        private GameObject _ghost;
        private Renderer[] _ghostRenderers;
        private Vector2Int _cell;
        private bool _hasCell;
        private bool _valid;

        private InputAction _toggle;
        private InputAction _rotate;
        private InputAction _cancel;
        private InputAction _primary;
        private InputAction _secondary;

        public bool IsBuilding => _mode != Mode.Off;

        public event Action<bool> ModeChanged;
        public event Action<string> Notice;

        private void Awake()
        {
            _toggle = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/b");
            _rotate = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/r");
            _cancel = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
            _primary = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
            _secondary = new InputAction(type: InputActionType.Button, binding: "<Mouse>/rightButton");

            _toggle.performed += _ => Toggle();
            _rotate.performed += _ => Rotate();
            _cancel.performed += _ => CancelOrExit();
            _primary.performed += _ => OnPrimary();
            _secondary.performed += _ => OnSecondary();
        }

        private void OnEnable()
        {
            _toggle.Enable();
            _rotate.Enable();
            _cancel.Enable();
            _primary.Enable();
            _secondary.Enable();
        }

        private void OnDisable()
        {
            _toggle.Disable();
            _rotate.Disable();
            _cancel.Disable();
            _primary.Disable();
            _secondary.Disable();
        }

        private void OnDestroy()
        {
            _toggle.Dispose();
            _rotate.Dispose();
            _cancel.Dispose();
            _primary.Dispose();
            _secondary.Dispose();
        }

        public void Toggle() => SetBuilding(_mode == Mode.Off);

        public void SetBuilding(bool on)
        {
            if (!on) CancelPlacing();

            _mode = on ? Mode.Idle : Mode.Off;
            foreach (Behaviour behaviour in _disableWhileBuilding)
                behaviour.enabled = !on;

            ModeChanged?.Invoke(on);
            Notice?.Invoke(on ? "건축 모드: 가구를 고르거나, 놓인 가구를 클릭해 옮기세요 (우클릭: 판매)" : "건축 모드 종료");
        }

        // 카탈로그 버튼에서 부른다
        public void BeginPlacing(FurnitureDefinition definition)
        {
            if (_mode == Mode.Off) return;
            StartPlacing(definition, 0, false);
        }

        private void StartPlacing(FurnitureDefinition definition, int rotation, bool isMove)
        {
            CancelPlacing();

            _mode = Mode.Placing;
            _placing = definition;
            _rotation = rotation;
            _isMove = isMove;
            _hasCell = false;
            CreateGhost(definition);
            Notice?.Invoke($"{definition.DisplayName}: 클릭으로 놓기, R 회전, 우클릭 취소");
        }

        private void CancelPlacing()
        {
            if (_mode != Mode.Placing) return;

            // 집어든 가구를 취소하면 원래 자리로 돌려놓는다
            if (_isMove) _lot.Place(_placing, _moveOriginCell, _moveOriginRotation);

            DestroyGhost();
            _placing = null;
            _isMove = false;
            _mode = Mode.Idle;
        }

        private void CancelOrExit()
        {
            if (_mode == Mode.Placing) CancelPlacing();
            else if (_mode == Mode.Idle) SetBuilding(false);
        }

        private void Rotate()
        {
            if (_mode != Mode.Placing) return;
            _rotation = (_rotation + 1) & 3;
            _ghost.transform.rotation = Quaternion.Euler(0f, _rotation * 90f, 0f);
        }

        private void Update()
        {
            if (_mode != Mode.Placing) return;

            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 screen = mouse.position.ReadValue();
            if (PointerOverUI(screen))
            {
                _ghost.SetActive(false);
                _hasCell = false;
                return;
            }

            // 바닥 평면과의 교차로 구한다. 물리 레이캐스트가 아니라서 가구/벽에 가려지지 않는다
            var floor = new Plane(Vector3.up, _lot.GetCenter(Vector2Int.zero, Vector2Int.zero));
            Ray ray = _camera.ScreenPointToRay(screen);
            if (!floor.Raycast(ray, out float distance))
            {
                _ghost.SetActive(false);
                _hasCell = false;
                return;
            }

            Vector2Int size = LotGrid.RotatedSize(_placing.Size, _rotation);
            _cell = _lot.WorldToCell(ray.GetPoint(distance), size);
            _hasCell = true;

            _valid = _lot.CanPlace(_placing, _cell, _rotation)
                && !_lot.FootprintContains(_cell, size, _player.position);

            _ghost.SetActive(true);
            _ghost.transform.position = _lot.GetCenter(_cell, size);
            SetGhostMaterial(_valid ? _validGhost : _invalidGhost);
        }

        private void OnPrimary()
        {
            if (_mode == Mode.Off || PointerOverUI(Mouse.current.position.ReadValue())) return;

            if (_mode == Mode.Placing)
            {
                TryPlace();
                return;
            }

            // 놓인 가구를 클릭하면 집어 든다 (이동은 무료)
            PlacedFurniture furniture = PickFurniture();
            if (furniture == null) return;

            FurnitureDefinition definition = furniture.Definition;
            int rotation = furniture.Rotation;
            Vector2Int cell = furniture.Cell;
            _lot.Remove(furniture);

            StartPlacing(definition, rotation, true);
            _moveOriginCell = cell;
            _moveOriginRotation = rotation;
        }

        private void OnSecondary()
        {
            if (_mode == Mode.Off || PointerOverUI(Mouse.current.position.ReadValue())) return;

            if (_mode == Mode.Placing)
            {
                CancelPlacing();
                return;
            }

            PlacedFurniture furniture = PickFurniture();
            if (furniture == null) return;

            int refund = Mathf.RoundToInt(furniture.Definition.Price * _config.SellRefundFraction);
            string name = furniture.Definition.DisplayName;
            _lot.Remove(furniture);
            _wallet.AddCash(refund);
            Notice?.Invoke($"{name} 판매: 현금 {refund} 획득");
        }

        private void TryPlace()
        {
            if (!_hasCell || !_valid)
            {
                Notice?.Invoke("여기에는 놓을 수 없다");
                return;
            }

            // 이동은 무료, 새로 사는 가구만 값을 낸다
            if (!_isMove && !_wallet.TrySpend(_placing.Price))
            {
                Notice?.Invoke($"현금이 부족하다 ({_placing.Price} 필요)");
                return;
            }

            _lot.Place(_placing, _cell, _rotation);

            if (_isMove)
            {
                DestroyGhost();
                _placing = null;
                _isMove = false;
                _mode = Mode.Idle;
                Notice?.Invoke("옮겼다");
            }
            else
            {
                Notice?.Invoke($"{_placing.DisplayName} 구입 (-{_placing.Price})");
            }
        }

        // 클릭 시점에만 호출되므로 GetComponentInParent를 써도 된다
        private PlacedFurniture PickFurniture()
        {
            Vector2 screen = Mouse.current.position.ReadValue();
            if (!Physics.Raycast(_camera.ScreenPointToRay(screen), out RaycastHit hit, 100f)) return null;
            return hit.collider.GetComponentInParent<PlacedFurniture>();
        }

        private bool PointerOverUI(Vector2 screen) =>
            UiHit.IsOverUI(_uiDocument.rootVisualElement.panel, screen);

        private void CreateGhost(FurnitureDefinition definition)
        {
            _ghost = Instantiate(definition.Prefab);

            // 고스트는 보이기만 해야 하므로 충돌/상호작용/길찾기 장애물 등을 모두 제거한다
            foreach (Collider collider in _ghost.GetComponentsInChildren<Collider>())
                Destroy(collider);
            foreach (MonoBehaviour behaviour in _ghost.GetComponentsInChildren<MonoBehaviour>())
                Destroy(behaviour);
            foreach (var obstacle in _ghost.GetComponentsInChildren<UnityEngine.AI.NavMeshObstacle>())
                Destroy(obstacle);

            _ghostRenderers = _ghost.GetComponentsInChildren<Renderer>();
            _ghost.transform.rotation = Quaternion.Euler(0f, _rotation * 90f, 0f);
            _ghost.SetActive(false);
        }

        private void SetGhostMaterial(Material material)
        {
            foreach (Renderer renderer in _ghostRenderers)
                renderer.sharedMaterial = material;
        }

        private void DestroyGhost()
        {
            if (_ghost != null) Destroy(_ghost);
            _ghost = null;
            _ghostRenderers = null;
        }
    }
}
