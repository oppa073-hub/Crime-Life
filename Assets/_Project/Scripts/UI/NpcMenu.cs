using System;
using CrimeLife.Crime;
using CrimeLife.Interaction;
using CrimeLife.NPC;
using CrimeLife.Player;
using CrimeLife.Social;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // NPC를 클릭하면 뜨는 메뉴: 대화, 선물, 소매치기, 강도. 코드로 만든다(항목이 데이터에 따라 달라질 수 있어서).
    [RequireComponent(typeof(UIDocument))]
    public class NpcMenu : MonoBehaviour
    {
        [SerializeField] private ClickToMoveController _clickSource;
        [SerializeField] private Camera _camera;
        [SerializeField] private RelationshipBook _book;
        [SerializeField] private RelationshipConfig _relationshipConfig;
        [SerializeField] private PlayerSocialController _social;
        [SerializeField] private PlayerCrimeController _crime;
        [SerializeField] private PlayerInteractor _interactor;

        private VisualElement _root;
        private VisualElement _panel;
        private Label _title;
        private Button _gift;
        private NpcAgent _npc;
        private int _openedFrame = -1;
        private InputAction _press;
        private InputAction _escape;

        private void Awake()
        {
            _press = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
            _escape = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/escape");
            _press.performed += _ => OnPress();
            _escape.performed += _ => Hide();
        }

        private void OnEnable()
        {
            _press.Enable();
            _escape.Enable();
        }

        private void OnDisable()
        {
            _press.Disable();
            _escape.Disable();
        }

        private void OnDestroy()
        {
            _press.Dispose();
            _escape.Dispose();
            if (_clickSource != null) _clickSource.NpcClicked -= Open;
        }

        private void Start()
        {
            var document = GetComponent<UIDocument>();
            _root = document.rootVisualElement;
            // 같은 패널을 쓰는 이름표 위에 그려지도록 순서를 올린다
            document.sortingOrder = 10;
            if (_camera == null) _camera = Camera.main;

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.width = 190f;
            _panel.style.paddingLeft = _panel.style.paddingRight = 8f;
            _panel.style.paddingTop = _panel.style.paddingBottom = 8f;
            _panel.style.backgroundColor = new Color(0f, 0f, 0f, 0.75f);
            _panel.style.display = DisplayStyle.None;
            _root.Add(_panel);

            _title = new Label();
            _title.style.color = Color.white;
            _title.style.fontSize = 16;
            _title.style.marginBottom = 6f;
            _panel.Add(_title);

            AddButton("대화", () => _social.Request(_npc, SocialKind.Chat));
            _gift = AddButton($"선물 ({_social.GiftCost})", () => _social.Request(_npc, SocialKind.Gift));
            AddButton("소매치기", () => _crime.Request(_npc, CrimeKind.Pickpocket));
            AddButton("강도", () => _crime.Request(_npc, CrimeKind.Mugging));
            AddButton("닫기", null);

            _clickSource.NpcClicked += Open;
        }

        private Button AddButton(string text, Action action)
        {
            var button = new Button(() =>
            {
                if (action != null)
                {
                    // 하던 일을 멈추고 새 행동 하나만 한다
                    _interactor.Cancel();
                    _social.Cancel();
                    _crime.Cancel();
                    action();
                }
                Hide();
            }) { text = text };
            _panel.Add(button);
            return button;
        }

        private void Open(NpcAgent npc)
        {
            _npc = npc;
            float friendship = _book.Get(npc);
            RelationshipTier tier = RelationshipLogic.GetTier(friendship, _relationshipConfig.Settings);
            string personality = string.IsNullOrEmpty(npc.Personality) ? "" : $"  [{npc.Personality}]";
            _title.text = $"{npc.DisplayName}{personality}\n{RelationshipText.TierName(tier)} ({Mathf.RoundToInt(friendship)})";

            // NPC 머리 오른쪽에 띄운다
            Vector3 head = npc.transform.position + Vector3.up * 2f;
            Vector2 pos = RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, head, _camera);
            float maxX = Mathf.Max(0f, _root.resolvedStyle.width - 230f);
            // 메뉴 높이(제목 2줄 + 버튼 5개)만큼 여유를 둬서 화면 아래로 잘리지 않게 한다
            float maxY = Mathf.Max(0f, _root.resolvedStyle.height - 300f);
            _panel.style.left = Mathf.Clamp(pos.x + 30f, 0f, maxX);
            _panel.style.top = Mathf.Clamp(pos.y - 20f, 0f, maxY);

            _panel.style.display = DisplayStyle.Flex;
            // 메뉴를 연 바로 그 클릭이 "바깥 클릭"으로 보고 닫히지 않게 한다
            _openedFrame = Time.frameCount;
        }

        private void Hide()
        {
            if (_panel != null) _panel.style.display = DisplayStyle.None;
            _npc = null;
        }

        private void OnPress()
        {
            if (_panel == null || _panel.resolvedStyle.display == DisplayStyle.None) return;
            if (Time.frameCount == _openedFrame) return;

            Vector2 panelPos = UiHit.ToPanel(_root.panel, Mouse.current.position.ReadValue());
            if (!_panel.worldBound.Contains(panelPos)) Hide();
        }
    }
}
