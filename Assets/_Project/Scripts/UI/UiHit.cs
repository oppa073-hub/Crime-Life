using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    public static class UiHit
    {
        // 화면 좌표(아래가 원점)가 UI 요소 위인지. 클릭이 UI를 뚫고 게임 월드로 새는 걸 막는 데 쓴다.
        // 같은 PanelSettings를 쓰는 UIDocument는 한 패널을 공유하므로 어느 문서의 패널을 넘겨도 된다.
        public static bool IsOverUI(IPanel panel, Vector2 screen) =>
            panel != null && panel.Pick(ToPanel(panel, screen)) != null;

        // 화면 좌표는 아래가 원점이고, UI Toolkit 패널 좌표는 위가 원점이다
        public static Vector2 ToPanel(IPanel panel, Vector2 screen) =>
            RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screen.x, Screen.height - screen.y));
    }
}
