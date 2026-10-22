using UnityEngine;

namespace CrimeLife.Crime
{
    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public static class WitnessLogic
    {
        // 경계심이 높을수록 신고한다. 범죄가 들통난 경우(실패)는 multiplier를 키워 더 잘 눈치채게 한다.
        public static bool Reports(float alertness, float multiplier, float roll) =>
            roll < Mathf.Clamp01(alertness * multiplier);
    }
}
