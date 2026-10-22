using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    // 월드 좌표 위에 UI Toolkit 라벨을 띄우는 공용 처리. NPC 이름표와 건물 이름표가 같이 쓴다.
    // 한글 폰트를 따로 준비하지 않아도 되고, 매 프레임 할당이 없다.
    public static class WorldLabelUtil
    {
        public static Label Create(VisualElement root, string text, Color textColor, int fontSize)
        {
            var label = new Label(text);
            label.pickingMode = PickingMode.Ignore;
            label.style.position = Position.Absolute;
            label.style.color = textColor;
            label.style.fontSize = fontSize;
            // 외곽선은 작은 글자를 어둡게 뭉개서, 반투명 배경으로 가독성을 확보한다
            label.style.backgroundColor = new Color(0f, 0f, 0f, 0.55f);
            label.style.paddingLeft = label.style.paddingRight = 6f;
            label.style.paddingTop = label.style.paddingBottom = 2f;
            label.style.borderTopLeftRadius = label.style.borderTopRightRadius = 4f;
            label.style.borderBottomLeftRadius = label.style.borderBottomRightRadius = 4f;
            label.style.translate = new Translate(Length.Percent(-50f), Length.Percent(-100f));
            root.Add(label);
            return label;
        }

        public static void Place(VisualElement root, Label label, Camera camera, Vector3 world)
        {
            // 카메라 뒤쪽은 화면 좌표가 뒤집혀 나오므로 숨긴다
            bool visible = camera.WorldToScreenPoint(world).z > 0f;
            label.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) return;

            Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(root.panel, world, camera);
            label.style.left = panelPos.x;
            label.style.top = panelPos.y;
        }
    }
}
