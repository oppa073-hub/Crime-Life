using System;
using UnityEngine;

namespace CrimeLife.Crime
{
    // 수배의 최소 버전: 범죄를 저지르면 오르고 시간이 지나면 식는다.
    // 경찰 반응은 아직 없고, 나중에 이 값(Heat)을 읽어서 붙인다.
    public class HeatTracker : MonoBehaviour
    {
        public const float MaxHeat = 100f;

        [SerializeField] private CrimeConfig _config;

        private float _heat;
        private int _lastShown;

        public float Heat => _heat;

        // 표시용 정수가 바뀔 때만 알려서 UI가 매 프레임 갱신되지 않게 한다
        public event Action<int> HeatChanged;

        public void Add(float amount)
        {
            _heat = Mathf.Clamp(_heat + amount, 0f, MaxHeat);
            Notify();
        }

        public void Clear()
        {
            _heat = 0f;
            Notify();
        }

        private void Update()
        {
            if (_heat <= 0f) return;
            _heat = Mathf.Max(0f, _heat - _config.HeatDecayPerSecond * Time.deltaTime);
            Notify();
        }

        private void Notify()
        {
            int shown = Mathf.CeilToInt(_heat);
            if (shown == _lastShown) return;
            _lastShown = shown;
            HeatChanged?.Invoke(shown);
        }
    }
}
