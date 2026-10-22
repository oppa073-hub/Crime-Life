using System;
using UnityEngine;

namespace CrimeLife.Core
{
    // NPC 일과표와 UI가 공유하는 게임 시계. 0~24시를 순환한다.
    public class GameClock : MonoBehaviour
    {
        [SerializeField, Range(0f, 24f)] private float _startHour = 7f;
        [Tooltip("실시간 1초당 흐르는 게임 시간(시간 단위)")]
        [SerializeField] private float _gameHoursPerRealSecond = 0.05f;

        private float _hour;
        private int _lastMinute;

        public float Hour => _hour;
        public int MinuteOfDay => _lastMinute;

        // UI가 매 프레임 문자열을 만들지 않도록 분이 바뀔 때만 알린다
        public event Action<int> MinuteChanged;

        private void Awake()
        {
            _hour = _startHour;
            _lastMinute = (int)(_hour * 60f);
        }

        private void Update()
        {
            _hour = Mathf.Repeat(_hour + Time.deltaTime * _gameHoursPerRealSecond, 24f);

            int minute = (int)(_hour * 60f);
            if (minute == _lastMinute) return;

            _lastMinute = minute;
            MinuteChanged?.Invoke(minute);
        }
    }
}
