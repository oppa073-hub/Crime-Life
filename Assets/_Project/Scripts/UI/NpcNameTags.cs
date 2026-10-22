using CrimeLife.NPC;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // NPC 머리 위에 이름을 띄운다. UI Toolkit 라벨을 화면 좌표로 옮기는 방식이라
    // 한글 폰트를 따로 준비하지 않아도 된다.
    [RequireComponent(typeof(UIDocument))]
    public class NpcNameTags : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private NpcAgent[] _npcs;
        [SerializeField] private float _heightOffset = 2.4f;

        private Label[] _labels;
        private VisualElement _root;

        private void Start()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            if (_camera == null) _camera = Camera.main;

            _labels = new Label[_npcs.Length];
            for (int i = 0; i < _npcs.Length; i++)
            {
                var label = new Label(_npcs[i].DisplayName);
                label.pickingMode = PickingMode.Ignore;
                label.style.position = Position.Absolute;
                label.style.color = Color.white;
                label.style.fontSize = 18;
                // 외곽선은 작은 글자를 어둡게 뭉개서, 반투명 배경으로 가독성을 확보한다
                label.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
                label.style.paddingLeft = label.style.paddingRight = 6f;
                label.style.paddingTop = label.style.paddingBottom = 2f;
                label.style.borderTopLeftRadius = label.style.borderTopRightRadius = 4f;
                label.style.borderBottomLeftRadius = label.style.borderBottomRightRadius = 4f;
                label.style.translate = new Translate(Length.Percent(-50f), Length.Percent(-100f));
                _root.Add(label);
                _labels[i] = label;
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _labels.Length; i++)
            {
                Vector3 world = _npcs[i].transform.position + Vector3.up * _heightOffset;

                // 카메라 뒤쪽 NPC는 화면 좌표가 뒤집혀 나오므로 숨긴다
                bool visible = _camera.WorldToScreenPoint(world).z > 0f;
                _labels[i].style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                if (!visible) continue;

                Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, world, _camera);
                _labels[i].style.left = panelPos.x;
                _labels[i].style.top = panelPos.y;
            }
        }
    }
}
