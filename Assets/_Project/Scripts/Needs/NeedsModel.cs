using System;
using UnityEngine;

namespace CrimeLife.Needs
{
    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public class NeedsModel
    {
        public const float MaxValue = 100f;

        private static readonly int Count = Enum.GetValues(typeof(NeedType)).Length;

        private readonly NeedSettings[] _settings;
        private readonly float[] _values;
        private readonly float _maxCrimePenaltyPerNeed;

        public event Action<NeedType, float> ValueChanged;
        public event Action<NeedType> BecameCritical;

        public NeedsModel(NeedSettings[] settings, float maxCrimePenaltyPerNeed)
        {
            if (settings == null || settings.Length != Count)
                throw new ArgumentException($"settings는 {Count}개여야 한다.");

            _settings = settings;
            _maxCrimePenaltyPerNeed = maxCrimePenaltyPerNeed;
            _values = new float[Count];
            for (int i = 0; i < Count; i++)
                _values[i] = Mathf.Clamp(settings[i].StartValue, 0f, MaxValue);
        }

        public float Get(NeedType type) => _values[(int)type];

        public float[] ToArray() => (float[])_values.Clone();

        // 저장된 값 복원. 개수가 다르면(저장 형식이 바뀐 경우) 무시하고 현재 값을 유지한다.
        public void Restore(float[] values)
        {
            if (values == null || values.Length != Count) return;

            for (int i = 0; i < Count; i++)
            {
                _values[i] = Mathf.Clamp(values[i], 0f, MaxValue);
                ValueChanged?.Invoke((NeedType)i, _values[i]);
            }
        }

        public bool IsCritical(NeedType type) =>
            _values[(int)type] <= _settings[(int)type].CriticalThreshold;

        public void Tick(float gameHours)
        {
            for (int i = 0; i < Count; i++)
                Change((NeedType)i, -_settings[i].DecayPerHour * gameHours);
        }

        // 음식, 수면, 범죄 결과(스트레스 증가 등)가 모두 이 경로로 들어온다.
        public void Add(NeedType type, float delta) => Change(type, delta);

        // 생활 → 범죄 훅: 성공률에 곱할 보정값(0~1). 위급한 욕구가 많고 깊을수록 낮아진다.
        public float GetCrimeModifier()
        {
            float modifier = 1f;
            for (int i = 0; i < Count; i++)
            {
                float threshold = _settings[i].CriticalThreshold;
                if (threshold <= 0f || _values[i] > threshold) continue;

                float severity = (threshold - _values[i]) / threshold;
                modifier -= severity * _maxCrimePenaltyPerNeed;
            }
            return Mathf.Max(0f, modifier);
        }

        private void Change(NeedType type, float delta)
        {
            int index = (int)type;
            bool wasCritical = IsCritical(type);
            float previous = _values[index];
            float next = Mathf.Clamp(previous + delta, 0f, MaxValue);
            if (Mathf.Approximately(previous, next)) return;

            _values[index] = next;
            ValueChanged?.Invoke(type, next);

            if (!wasCritical && IsCritical(type))
                BecameCritical?.Invoke(type);
        }
    }
}
