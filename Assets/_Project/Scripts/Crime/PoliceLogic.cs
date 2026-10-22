using UnityEngine;

namespace CrimeLife.Crime
{
    public enum PoliceState
    {
        Idle,
        Chasing,
    }

    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public static class PoliceLogic
    {
        // 추격 시작과 포기 기준을 다르게 둬서(히스테리시스) 열기가 임계치 근처에서 오르내려도
        // 경찰이 추격/복귀를 매 프레임 반복하지 않게 한다.
        public static PoliceState Next(PoliceState current, float heat, float chaseHeat, float releaseHeat)
        {
            if (current == PoliceState.Idle)
                return heat >= chaseHeat ? PoliceState.Chasing : PoliceState.Idle;

            return heat < releaseHeat ? PoliceState.Idle : PoliceState.Chasing;
        }

        public static int CalculateFine(int money, float fraction) =>
            Mathf.Clamp(Mathf.RoundToInt(money * fraction), 0, money);
    }
}
