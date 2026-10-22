using System.Collections.Generic;
using CrimeLife.Core;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NpcAgent : MonoBehaviour
    {
        [SerializeField] private NpcProfile _profile;
        [SerializeField] private GameClock _clock;
        [SerializeField] private Waypoint[] _waypoints;
        [SerializeField] private float _sampleDistance = 3f;

        private NavMeshAgent _agent;
        private readonly Dictionary<string, Waypoint> _waypointById = new Dictionary<string, Waypoint>();
        private int _currentIndex = -2;
        private int _money;
        private float _waryUntil;

        // 범죄를 당한 직후에는 경계해서 다시 노릴 수 없다
        public bool IsWary => Time.time < _waryUntil;

        public string DisplayName => _profile.DisplayName;
        public float Alertness => _profile.Alertness;
        public int Money => _money;
        public string CurrentActivity { get; private set; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _money = _profile.StartingMoney;

            for (int i = 0; i < _waypoints.Length; i++)
                _waypointById[_waypoints[i].Id] = _waypoints[i];
        }

        public void BecomeWary(float seconds)
        {
            _waryUntil = Time.time + seconds;
        }

        // 범죄 시스템 연결 지점: 실제로 빼앗긴 금액을 돌려준다
        public int TakeMoney(int amount)
        {
            int taken = Mathf.Clamp(amount, 0, _money);
            _money -= taken;
            return taken;
        }

        private void Update()
        {
            // 일과표 항목이 바뀐 순간에만 이동 명령을 내린다
            int index = ScheduleLogic.GetActiveIndex(_profile.Schedule, _clock.Hour);
            if (index == _currentIndex) return;
            _currentIndex = index;
            if (index < 0) return;

            ScheduleEntry entry = _profile.Schedule[index];
            CurrentActivity = entry.Label;

            if (!_waypointById.TryGetValue(entry.WaypointId, out Waypoint waypoint))
            {
                Debug.LogWarning($"Waypoint '{entry.WaypointId}'를 찾을 수 없다.", this);
                return;
            }

            if (NavMesh.SamplePosition(waypoint.transform.position, out NavMeshHit hit, _sampleDistance, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }
    }
}
