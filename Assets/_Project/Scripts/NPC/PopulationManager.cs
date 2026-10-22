using System;
using System.Collections.Generic;
using CrimeLife.Core;
using CrimeLife.Social;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.NPC
{
    // 도시의 사람들을 관리한다: 고정 주요 NPC + 계속 바뀌는 랜덤 NPC + 플레이어와 관계를 맺어 저장된 "아는 사람".
    //  - 랜덤 NPC는 정해진 수(기본 20명)를 유지하며, 일정 시간마다 일부가 떠나고 새 사람이 온다.
    //  - 관계를 맺은 랜덤 NPC는 아는 사람으로 저장되어, 도시에 없어도 부를 수 있고 가끔 다시 나타난다.
    public class PopulationManager : MonoBehaviour, ISaveable
    {
        [SerializeField] private NpcAgent _npcPrefab;
        [SerializeField] private PopulationConfig _config;
        [SerializeField] private NpcGeneratorConfig _generator;
        [SerializeField] private CharacterCatalog _catalog;
        [SerializeField] private GameClock _clock;
        [SerializeField] private WaypointDirectory _directory;
        [SerializeField] private RelationshipBook _book;
        [SerializeField] private Transform _player;
        [Tooltip("고정 주요 NPC. 항상 도시에 있다")]
        [SerializeField] private NpcProfile[] _mainProfiles;
        [Tooltip("랜덤 NPC의 집이 될 수 있는 장소 역할")]
        [SerializeField] private string _residenceRole = "Residence";

        private readonly List<NpcAgent> _active = new List<NpcAgent>();
        private readonly Dictionary<string, NpcData> _known = new Dictionary<string, NpcData>();
        private readonly List<NpcData> _mainData = new List<NpcData>();
        private System.Random _rng;
        private float _nextRotateAt;

        public IReadOnlyList<NpcAgent> Active => _active;

        public event Action<NpcAgent> Spawned;
        public event Action<NpcAgent> Despawned;
        public event Action<string> Notice;
        public event Action ContactsChanged;

        private void Awake()
        {
            _rng = new System.Random();
        }

        private void Start()
        {
            foreach (NpcProfile profile in _mainProfiles)
            {
                NpcData data = profile.ToData();
                _mainData.Add(data);
                Spawn(data, false);
            }

            FillRandoms(_config.KnownInitialChance);
            _nextRotateAt = _clock.TotalHours + _config.RotateIntervalHours;
            if (_book != null) _book.Changed += OnRelationshipChanged;
        }

        private void OnDestroy()
        {
            if (_book != null) _book.Changed -= OnRelationshipChanged;
        }

        private void Update()
        {
            if (_clock.TotalHours < _nextRotateAt) return;
            _nextRotateAt = _clock.TotalHours + _config.RotateIntervalHours;
            Rotate();
        }

        // ---------- 아는 사람 / 부르기 ----------

        // 전화번호부: 고정 주요 NPC + 아는 사람
        public List<NpcData> GetContacts()
        {
            var contacts = new List<NpcData>(_mainData);
            contacts.AddRange(_known.Values);
            return contacts;
        }

        // 플레이어에게 오게 한다. 도시에 없는 아는 사람은 집에서 새로 나타난다.
        public bool Summon(string id)
        {
            NpcAgent agent = FindActive(id);
            if (agent == null)
            {
                if (!_known.TryGetValue(id, out NpcData data)) return false;
                agent = Spawn(data, true);
            }

            agent.CallTo(_player, _config.SummonSeconds);
            Notice?.Invoke($"{agent.DisplayName}에게 연락했다. 곧 올 것이다");
            return true;
        }

        private void OnRelationshipChanged(NpcAgent npc, float value, float delta)
        {
            if (npc == null || npc.IsMain || _known.ContainsKey(npc.Id)) return;

            // 한 번이라도 관계가 생기면(좋은 쪽이든 나쁜 쪽이든) 플레이어를 기억하는 사람이 된다
            _known[npc.Id] = npc.Data;
            Notice?.Invoke($"{npc.DisplayName}의 연락처를 알게 됐다");
            ContactsChanged?.Invoke();
        }

        // ---------- 랜덤 NPC 유지 ----------

        // 빈 자리를 채운다. 아는 사람은 한 명씩 확률(knownChance)로 먼저 자리를 차지하고, 나머지는 새로 만든다.
        // (빈 자리마다 확률을 주면 아는 사람이 적을 때 거의 항상 뽑혀서 "가끔 나타난다"가 안 된다)
        private void FillRandoms(float knownChance)
        {
            int missing = _config.RandomCount - CountRandoms();
            if (missing <= 0) return;

            var returning = new List<NpcData>();
            foreach (NpcData known in _known.Values)
            {
                if (returning.Count >= missing) break;
                if (FindActive(known.Id) == null && PopulationLogic.ShouldUseKnown(_rng, knownChance, 1))
                    returning.Add(known);
            }
            foreach (NpcData known in returning) Spawn(known, false);

            while (CountRandoms() < _config.RandomCount) SpawnOneRandom();
        }

        private void Rotate()
        {
            var candidates = new List<RotationCandidate>();
            var randoms = new List<NpcAgent>();
            foreach (NpcAgent agent in _active)
            {
                if (agent == null || agent.IsMain) continue;

                candidates.Add(new RotationCandidate
                {
                    Index = randoms.Count,
                    DistanceToPlayer = FlatDistance(agent.transform.position, _player.position),
                    Protected = agent.IsSummoned,
                });
                randoms.Add(agent);
            }

            // 정해진 수보다 많아졌다면(부른 사람이 더해진 경우) 그만큼 더 내보낸다
            int count = _config.RotateCount + Mathf.Max(0, randoms.Count - _config.RandomCount);
            foreach (int index in PopulationLogic.SelectForRotation(candidates, _config.MinDespawnDistance, count, _rng))
                Despawn(randoms[index]);

            FillRandoms(_config.KnownAppearChance);
        }

        private void SpawnOneRandom()
        {
            NpcData data = NpcGenerator.Generate(
                _rng, _generator.Settings, UsedIds(), UsedNames(),
                _directory.GetIdsByRole(_residenceRole), _catalog.Count);

            Spawn(data, false);
        }

        private NpcAgent Spawn(NpcData data, bool atHome)
        {
            Vector3 position = FindSpawnPosition(data, atHome);
            NpcAgent agent = Instantiate(_npcPrefab, position, Quaternion.identity, transform);
            agent.name = "Npc_" + data.DisplayName;
            agent.Initialize(data, _clock, _directory);

            CharacterCatalog.Entry look = _catalog.Get(data.AppearanceIndex);
            agent.GetComponentInChildren<CharacterVisual>().Build(look.Prefab, look.PlaceholderColor);

            _active.Add(agent);
            Spawned?.Invoke(agent);
            return agent;
        }

        private void Despawn(NpcAgent agent)
        {
            // 떠나기 전에 소지금 같은 변한 값을 데이터에 돌려 쓴다 (아는 사람이면 이 데이터가 저장된다)
            agent.SyncToData();
            _active.Remove(agent);
            Despawned?.Invoke(agent);
            Destroy(agent.gameObject);
        }

        // ---------- 위치 ----------

        private Vector3 FindSpawnPosition(NpcData data, bool atHome)
        {
            Vector3 basePosition = atHome ? HomePosition(data) : SchedulePosition(data);

            // 플레이어 눈앞에서 갑자기 나타나지 않게, 가까우면 집(그래도 가까우면 그대로)에서 시작한다
            if (!atHome && FlatDistance(basePosition, _player.position) < _config.MinSpawnDistance)
            {
                Vector3 home = HomePosition(data);
                if (FlatDistance(home, _player.position) >= _config.MinSpawnDistance) basePosition = home;
            }

            // 같은 장소에 겹쳐 서지 않게 조금 흩어 놓는다
            Vector3 jitter = new Vector3((float)_rng.NextDouble() * 4f - 2f, 0f, (float)_rng.NextDouble() * 4f - 2f);
            return NavMesh.SamplePosition(basePosition + jitter, out NavMeshHit hit, 4f, NavMesh.AllAreas)
                ? hit.position
                : basePosition;
        }

        private Vector3 SchedulePosition(NpcData data)
        {
            int index = ScheduleLogic.GetActiveIndex(data.Schedule, _clock.Hour);
            if (index < 0) return HomePosition(data);

            string id = data.Schedule[index].WaypointId == NpcGenerator.HomeToken ? data.HomeId : data.Schedule[index].WaypointId;
            return _directory.TryGet(id, NpcData.SeedFor(data.Id), out Waypoint waypoint)
                ? waypoint.transform.position
                : HomePosition(data);
        }

        private Vector3 HomePosition(NpcData data) =>
            _directory.TryGet(data.HomeId, NpcData.SeedFor(data.Id), out Waypoint home)
                ? home.transform.position
                : _player.position;

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        // ---------- 조회 ----------

        private NpcAgent FindActive(string id)
        {
            foreach (NpcAgent agent in _active)
            {
                if (agent != null && agent.Id == id) return agent;
            }
            return null;
        }

        private int CountRandoms()
        {
            int count = 0;
            foreach (NpcAgent agent in _active)
            {
                if (agent != null && !agent.IsMain) count++;
            }
            return count;
        }

        private HashSet<string> UsedIds()
        {
            var ids = new HashSet<string>(_known.Keys);
            foreach (NpcAgent agent in _active) ids.Add(agent.Id);
            foreach (NpcData main in _mainData) ids.Add(main.Id);
            return ids;
        }

        private HashSet<string> UsedNames()
        {
            var names = new HashSet<string>();
            foreach (NpcData known in _known.Values) names.Add(known.DisplayName);
            foreach (NpcAgent agent in _active) names.Add(agent.DisplayName);
            foreach (NpcData main in _mainData) names.Add(main.DisplayName);
            return names;
        }

        // ---------- 저장 ----------

        public void Save(SaveData data)
        {
            data.KnownNpcs = new List<NpcData>();
            foreach (NpcData known in _known.Values)
            {
                NpcAgent agent = FindActive(known.Id);
                if (agent != null) agent.SyncToData();
                data.KnownNpcs.Add(known);
            }
        }

        public void Load(SaveData data)
        {
            _known.Clear();
            foreach (NpcData known in data.KnownNpcs) _known[known.Id] = known;
            ContactsChanged?.Invoke();
        }
    }
}
