using CrimeLife.NPC;
using CrimeLife.Social;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // 연락처: 고정 주요 NPC와 관계를 맺은 사람을 보여 주고, 부르면 플레이어에게 오게 한다.
    // 도시에 없는 아는 사람도 부를 수 있다. 친밀도가 "아는 사이" 이상이어야 부를 수 있다.
    [RequireComponent(typeof(UIDocument))]
    public class PhoneMenu : MonoBehaviour
    {
        [SerializeField] private PopulationManager _population;
        [SerializeField] private RelationshipBook _book;
        [SerializeField] private RelationshipConfig _relationshipConfig;

        private VisualElement _panel;
        private ScrollView _list;
        private InputAction _toggle;

        private void Awake()
        {
            _toggle = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/p");
            _toggle.performed += _ => Toggle();
        }

        private void OnEnable() => _toggle.Enable();

        private void OnDisable() => _toggle.Disable();

        private void OnDestroy()
        {
            _toggle.Dispose();
            if (_population != null) _population.ContactsChanged -= RebuildIfOpen;
        }

        private void Start()
        {
            var document = GetComponent<UIDocument>();
            document.sortingOrder = 10;
            VisualElement root = document.rootVisualElement;

            _panel = new VisualElement();
            _panel.style.position = Position.Absolute;
            _panel.style.left = Length.Percent(50f);
            _panel.style.top = 80f;
            _panel.style.width = 340f;
            _panel.style.translate = new Translate(Length.Percent(-50f), 0f);
            _panel.style.paddingLeft = _panel.style.paddingRight = 10f;
            _panel.style.paddingTop = _panel.style.paddingBottom = 10f;
            _panel.style.backgroundColor = new Color(0f, 0f, 0f, 0.8f);
            _panel.style.display = DisplayStyle.None;
            root.Add(_panel);

            var title = new Label("연락처  (P로 닫기)");
            title.style.color = Color.white;
            title.style.fontSize = 18;
            title.style.marginBottom = 6f;
            _panel.Add(title);

            _list = new ScrollView { style = { maxHeight = 360f } };
            _panel.Add(_list);

            _population.ContactsChanged += RebuildIfOpen;
        }

        private void Toggle()
        {
            bool open = _panel.resolvedStyle.display == DisplayStyle.None;
            _panel.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            if (open) Rebuild();
        }

        private void RebuildIfOpen()
        {
            if (_panel != null && _panel.resolvedStyle.display != DisplayStyle.None) Rebuild();
        }

        private void Rebuild()
        {
            _list.Clear();
            RelationshipSettings settings = _relationshipConfig.Settings;

            foreach (NpcData contact in _population.GetContacts())
            {
                float friendship = _book.Get(contact.Id, contact.InitialFriendship);
                RelationshipTier tier = RelationshipLogic.GetTier(friendship, settings);
                bool canCall = friendship >= settings.AcquaintanceFrom;

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4f;

                string personality = string.IsNullOrEmpty(contact.Personality) ? "" : $" [{contact.Personality}]";
                var label = new Label($"{contact.DisplayName}{personality}\n{RelationshipText.TierName(tier)} ({Mathf.RoundToInt(friendship)})");
                label.style.color = Color.white;
                label.style.flexGrow = 1f;
                label.style.whiteSpace = WhiteSpace.Normal;
                row.Add(label);

                string id = contact.Id;
                var call = new Button(() =>
                {
                    _population.Summon(id);
                    _panel.style.display = DisplayStyle.None;
                }) { text = canCall ? "부르기" : "아직 서먹함" };
                call.SetEnabled(canCall);
                row.Add(call);

                _list.Add(row);
            }
        }
    }
}
