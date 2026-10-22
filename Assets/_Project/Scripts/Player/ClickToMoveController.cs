using System;
using CrimeLife.Crime;
using CrimeLife.Interaction;
using CrimeLife.NPC;
using CrimeLife.Social;
using CrimeLife.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CrimeLife.Player
{
    // 심즈식 이동: 땅을 클릭하면 NavMesh 경로로 걸어가고, 상호작용 오브젝트를 클릭하면 접근 후 사용한다.
    // NPC를 클릭하면 직접 행동하지 않고 메뉴를 열도록 알린다(대화/선물/범죄 중 고르기).
    [RequireComponent(typeof(NavMeshAgent), typeof(PlayerInteractor), typeof(PlayerCrimeController))]
    public class ClickToMoveController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [Tooltip("UI 위에서 한 클릭이 월드로 새지 않게 가려내는 데 쓴다 (같은 PanelSettings를 쓰는 아무 UIDocument)")]
        [SerializeField] private UIDocument _uiDocument;
        [Tooltip("사교 컨트롤러. NPC가 없는 씬에서는 비워 둔다")]
        [SerializeField] private PlayerSocialController _social;
        [Tooltip("클릭 지점에서 가장 가까운 NavMesh를 찾는 허용 거리")]
        [SerializeField] private float _sampleDistance = 1.5f;
        [SerializeField] private float _maxRayDistance = 200f;

        private NavMeshAgent _agent;
        private PlayerInteractor _interactor;
        private PlayerCrimeController _crime;
        private InputAction _click;

        public event Action<NpcAgent> NpcClicked;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _interactor = GetComponent<PlayerInteractor>();
            _crime = GetComponent<PlayerCrimeController>();
            if (_camera == null) _camera = Camera.main;

            // Update 폴링 대신 콜백으로 받아서, 클릭 시점에만 GetComponent 계열 호출이 일어나게 한다
            _click = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
        }

        private void OnEnable()
        {
            _click.performed += OnClick;
            _click.Enable();
        }

        private void OnDisable()
        {
            _click.Disable();
            _click.performed -= OnClick;
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 screen = mouse.position.ReadValue();
            if (_uiDocument != null && UiHit.IsOverUI(_uiDocument.rootVisualElement.panel, screen)) return;

            Ray ray = _camera.ScreenPointToRay(screen);
            if (!Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance)) return;

            var interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                CancelNpcActions();
                _interactor.Request(interactable);
                return;
            }

            var npc = hit.collider.GetComponentInParent<NpcAgent>();
            if (npc != null)
            {
                NpcClicked?.Invoke(npc);
                return;
            }

            _interactor.Cancel();
            CancelNpcActions();
            // 캐릭터/벽을 클릭해도 근처 바닥으로 보정되도록 NavMesh에 스냅한다
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, _sampleDistance, NavMesh.AllAreas))
                _agent.SetDestination(navHit.position);
        }

        private void CancelNpcActions()
        {
            _crime.Cancel();
            if (_social != null) _social.Cancel();
        }
    }
}
