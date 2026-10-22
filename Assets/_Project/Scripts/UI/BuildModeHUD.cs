using CrimeLife.Build;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // 건축 모드 버튼, 가구 카탈로그, 안내 문구. UXML 없이 코드로 만든다(카탈로그 항목 수가 데이터로 정해져서).
    [RequireComponent(typeof(UIDocument))]
    public class BuildModeHUD : MonoBehaviour
    {
        [SerializeField] private BuildModeController _controller;
        [SerializeField] private FurnitureCatalog _catalog;

        private Button _toggle;
        private VisualElement _panel;
        private Label _status;

        private void Start()
        {
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;

            _toggle = new Button(_controller.Toggle) { text = "건축 모드 (B)" };
            _toggle.style.position = Position.Absolute;
            _toggle.style.right = 16f;
            _toggle.style.top = 16f;
            _toggle.style.width = 160f;
            root.Add(_toggle);

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.right = 16f;
            _panel.style.top = 56f;
            _panel.style.width = 220f;
            _panel.style.paddingLeft = _panel.style.paddingRight = 8f;
            _panel.style.paddingTop = _panel.style.paddingBottom = 8f;
            _panel.style.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
            _panel.style.display = DisplayStyle.None;
            root.Add(_panel);

            foreach (FurnitureDefinition definition in _catalog.Items)
            {
                FurnitureDefinition item = definition;
                var button = new Button(() => _controller.BeginPlacing(item))
                {
                    text = $"{item.DisplayName}  ({item.Price})"
                };
                _panel.Add(button);
            }

            var hint = new Label("R 회전 / 우클릭 취소·판매\n놓인 가구 클릭: 이동\nEsc: 취소·종료");
            hint.style.color = Color.white;
            hint.style.marginTop = 6f;
            hint.style.whiteSpace = WhiteSpace.Normal;
            _panel.Add(hint);

            _status = new Label();
            _status.pickingMode = PickingMode.Ignore;
            _status.style.position = Position.Absolute;
            _status.style.bottom = 24f;
            _status.style.left = 0f;
            _status.style.right = 0f;
            _status.style.unityTextAlign = TextAnchor.MiddleCenter;
            _status.style.color = Color.white;
            _status.style.fontSize = 20;
            root.Add(_status);

            _controller.ModeChanged += OnModeChanged;
            _controller.Notice += OnNotice;
        }

        private void OnDestroy()
        {
            if (_controller == null) return;
            _controller.ModeChanged -= OnModeChanged;
            _controller.Notice -= OnNotice;
        }

        private void OnModeChanged(bool on)
        {
            _panel.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
            _toggle.text = on ? "건축 모드 끄기 (B)" : "건축 모드 (B)";
        }

        private void OnNotice(string text) => _status.text = text;
    }
}
