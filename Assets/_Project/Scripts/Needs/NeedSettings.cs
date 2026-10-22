using System;

namespace CrimeLife.Needs
{
    [Serializable]
    public struct NeedSettings
    {
        public float StartValue;
        // 게임 시간 1시간당 감소량
        public float DecayPerHour;
        // 이 값 이하가 되면 위급 상태로 보고 범죄 성공률에 페널티를 준다
        public float CriticalThreshold;

        public NeedSettings(float startValue, float decayPerHour, float criticalThreshold)
        {
            StartValue = startValue;
            DecayPerHour = decayPerHour;
            CriticalThreshold = criticalThreshold;
        }
    }
}
