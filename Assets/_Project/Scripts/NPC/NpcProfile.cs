using UnityEngine;

namespace CrimeLife.NPC
{
    [CreateAssetMenu(fileName = "NpcProfile", menuName = "CrimeLife/NPC Profile")]
    public class NpcProfile : ScriptableObject
    {
        [Tooltip("저장 파일에서 NPC를 가리키는 이름. 비워 두면 에셋 이름을 쓴다")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName = "NPC";

        [Header("성격")]
        [Tooltip("높을수록 소매치기를 알아채기 쉽다 (범죄 시스템에서 사용)")]
        [SerializeField, Range(0f, 1f)] private float _alertness = 0.5f;

        [Tooltip("친화력: 높을수록 대화/선물로 친밀도가 잘 오른다")]
        [SerializeField, Range(0f, 1f)] private float _sociability = 0.5f;

        [Header("외형/집")]
        [Tooltip("캐릭터 카탈로그의 번호")]
        [SerializeField] private int _appearanceIndex;
        [Tooltip("일과표의 @Home이 가리키는 집 장소 Id")]
        [SerializeField] private string _homeId;

        [Header("관계")]
        [Tooltip("게임 시작 시 플레이어와의 친밀도 (0~100)")]
        [SerializeField, Range(0f, 100f)] private float _initialFriendship;

        [Header("소지금")]
        [SerializeField, Min(0)] private int _startingMoney = 100;

        [Header("일과표")]
        [SerializeField] private ScheduleEntry[] _schedule;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public float InitialFriendship => _initialFriendship;
        public string DisplayName => _displayName;
        public float Alertness => _alertness;
        public int StartingMoney => _startingMoney;
        public ScheduleEntry[] Schedule => _schedule;

        // 고정 주요 NPC도 랜덤 NPC와 같은 데이터 형태로 바꿔서 NpcAgent에 넘긴다
        public NpcData ToData() => new NpcData
        {
            Id = Id,
            DisplayName = _displayName,
            IsMain = true,
            Alertness = _alertness,
            Sociability = _sociability,
            Personality = NpcGenerator.PersonalityLabel(_alertness, _sociability),
            Money = _startingMoney,
            AppearanceIndex = _appearanceIndex,
            InitialFriendship = _initialFriendship,
            HomeId = _homeId,
            Schedule = (ScheduleEntry[])_schedule.Clone(),
        };
    }
}
