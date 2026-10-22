using CrimeLife.Core;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.NPC
{
    // NPC 한 명의 런타임 몸. 어떤 사람인지는 NpcData가 정하고(PopulationManager가 Initialize로 넘긴다),
    // 여기서는 일과표대로 움직이거나 플레이어의 부름에 응한다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcAgent : MonoBehaviour
    {
        [SerializeField] private float _sampleDistance = 3f;
        [Tooltip("부름을 받으면 플레이어에게 이 거리까지 다가온다")]
        [SerializeField] private float _callStopDistance = 2.5f;

        private NavMeshAgent _agent;
        private NpcData _data;
        private GameClock _clock;
        private WaypointDirectory _directory;
        private int _seed;
        private int _currentIndex = -2;
        private int _money;
        private float _waryUntil;
        private float _socialReadyAt;

        private Transform _callTarget;
        private float _callUntil;
        private float _callRepath;

        public NpcData Data => _data;
        public string Id => _data.Id;
        public string DisplayName => _data.DisplayName;
        public float Alertness => _data.Alertness;
        public float Sociability => _data.Sociability;
        public string Personality => _data.Personality;
        public float InitialFriendship => _data.InitialFriendship;
        public int Money => _money;
        public bool IsMain => _data.IsMain;
        public string CurrentActivity { get; private set; }

        // 범죄를 당한 직후에는 경계해서 다시 노릴 수 없다
        public bool IsWary => Time.time < _waryUntil;

        // 대화/선물을 연달아 해서 친밀도를 올리는 걸 막는다
        public bool IsOnSocialCooldown => Time.time < _socialReadyAt;

        // 플레이어의 부름에 응해 오는 중
        public bool IsSummoned => _callTarget != null;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(NpcData data, GameClock clock, WaypointDirectory directory)
        {
            _data = data;
            _clock = clock;
            _directory = directory;
            _money = data.Money;
            // 같은 NPC는 같은 이름의 장소가 여러 개여도 항상 같은 곳으로 가게 하는 고정 값
            _seed = NpcData.SeedFor(data.Id);
            _currentIndex = -2;
        }

        // 상태가 바뀐 데이터를 저장/순환 전에 돌려 쓰기 위해 현재 값을 데이터에 반영한다
        public void SyncToData()
        {
            _data.Money = _money;
        }

        public void BecomeWary(float seconds)
        {
            _waryUntil = Time.time + seconds;
        }

        public void StartSocialCooldown(float seconds)
        {
            _socialReadyAt = Time.time + seconds;
        }

        // 범죄 시스템 연결 지점: 실제로 빼앗긴 금액을 돌려준다
        public int TakeMoney(int amount)
        {
            int taken = Mathf.Clamp(amount, 0, _money);
            _money -= taken;
            return taken;
        }

        // 플레이어가 부르면 일정 시간 동안 일과표를 멈추고 다가온다
        public void CallTo(Transform target, float seconds)
        {
            _callTarget = target;
            _callUntil = Time.time + seconds;
            _callRepath = 0f;
            CurrentActivity = "부름에 응하는 중";
        }

        private void Update()
        {
            if (_data == null) return;

            if (_callTarget != null)
            {
                UpdateCall();
                return;
            }

            // 일과표 항목이 바뀐 순간에만 이동 명령을 내린다
            int index = ScheduleLogic.GetActiveIndex(_data.Schedule, _clock.Hour);
            if (index == _currentIndex) return;
            _currentIndex = index;
            if (index < 0) return;

            ScheduleEntry entry = _data.Schedule[index];
            CurrentActivity = entry.Label;

            string waypointId = entry.WaypointId == NpcGenerator.HomeToken ? _data.HomeId : entry.WaypointId;
            if (!_directory.TryGet(waypointId, _seed, out Waypoint waypoint))
            {
                Debug.LogWarning($"Waypoint '{waypointId}'를 찾을 수 없다.", this);
                return;
            }

            if (NavMesh.SamplePosition(waypoint.transform.position, out NavMeshHit hit, _sampleDistance, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }

        private void UpdateCall()
        {
            if (Time.time > _callUntil)
            {
                // 부름이 끝나면 일과표를 다시 따른다
                _callTarget = null;
                _currentIndex = -2;
                return;
            }

            _callRepath -= Time.deltaTime;
            if (_callRepath > 0f) return;
            _callRepath = 0.5f;

            Vector3 offset = _callTarget.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= _callStopDistance * _callStopDistance) _agent.ResetPath();
            else _agent.SetDestination(_callTarget.position);
        }
    }
}
