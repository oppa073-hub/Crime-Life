using CrimeLife.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrimeLife.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ClockHUD : MonoBehaviour
    {
        [SerializeField] private GameClock _clock;

        private Label _label;

        // GameClock.Awake 이후에 초기값을 읽도록 Start에서 초기화한다
        private void Start()
        {
            _label = GetComponent<UIDocument>().rootVisualElement.Q<Label>("clock");
            SetTime(_clock.MinuteOfDay);
            _clock.MinuteChanged += SetTime;
        }

        private void OnDestroy()
        {
            if (_clock != null) _clock.MinuteChanged -= SetTime;
        }

        private void SetTime(int minuteOfDay)
        {
            _label.text = $"{minuteOfDay / 60:00}:{minuteOfDay % 60:00}";
        }
    }
}
