using UnityEngine;

namespace CrimeLife.NPC
{
    [CreateAssetMenu(fileName = "NpcProfile", menuName = "CrimeLife/NPC Profile")]
    public class NpcProfile : ScriptableObject
    {
        [SerializeField] private string _displayName = "NPC";

        [Header("성격")]
        [Tooltip("높을수록 소매치기를 알아채기 쉽다 (범죄 시스템에서 사용)")]
        [SerializeField, Range(0f, 1f)] private float _alertness = 0.5f;

        [Header("소지금")]
        [SerializeField, Min(0)] private int _startingMoney = 100;

        [Header("일과표")]
        [SerializeField] private ScheduleEntry[] _schedule;

        public string DisplayName => _displayName;
        public float Alertness => _alertness;
        public int StartingMoney => _startingMoney;
        public ScheduleEntry[] Schedule => _schedule;
    }
}
