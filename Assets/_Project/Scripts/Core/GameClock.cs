using System;
using UnityEngine;

namespace CrimeLife.Core
{
    // NPC 일과표와 UI가 공유하는 게임 시계. 0~24시를 순환한다.
    public class GameClock : MonoBehaviour, ISaveable
    {
        [SerializeField, Range(0f, 24f)] private float _startHour = 7f;
        [Tooltip("실시간 1초당 흐르는 게임 시간(시간 단위)")]
        [SerializeField] private float _gameHoursPerRealSecond = 0.05f;

        private float _hour;
        private int _day;
        private int _lastMinute;

        public float Hour => _hour;
        public int Day => _day;
        // 청구서 기한처럼 며칠에 걸친 시간 계산에 쓰는 누적 시간
        public float TotalHours => _day * 24f + _hour;
        public int MinuteOfDay => _lastMinute;

        // UI가 매 프레임 문자열을 만들지 않도록 분이 바뀔 때만 알린다
        public event Action<int> MinuteChanged;

        private void Awake()
        {
            _hour = _startHour;
            _lastMinute = (int)(_hour * 60f);
        }

        public void Save(SaveData data)
        {
            data.Hour = _hour;
            data.Day = _day;
        }

        public void Load(SaveData data)
        {
            _hour = Mathf.Repeat(data.Hour, 24f);
            _day = Mathf.Max(0, data.Day);
            _lastMinute = (int)(_hour * 60f);
        }

        private void Update()
        {
            float next = _hour + Time.deltaTime * _gameHoursPerRealSecond;
            // 24시를 넘기면 다음 날로 넘어간다
            if (next >= 24f) _day++;
            _hour = Mathf.Repeat(next, 24f);

            int minute = (int)(_hour * 60f);
            if (minute == _lastMinute) return;

            _lastMinute = minute;
            MinuteChanged?.Invoke(minute);
        }
    }
}
