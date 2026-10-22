using System;
using CrimeLife.Needs;
using CrimeLife.NPC;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.Crime
{
    // NPC를 따라가서 사정거리 안에 들어오면 범죄를 시도하고, 결과를 생활 시스템에 되돌린다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerCrimeController : MonoBehaviour
    {
        [SerializeField] private CrimeConfig _config;
        [SerializeField] private NeedsController _needs;
        [SerializeField] private Wallet _wallet;
        [SerializeField] private HeatTracker _heat;
        [Tooltip("대상이 움직일 때 목적지를 다시 잡는 주기")]
        [SerializeField] private float _repathInterval = 0.25f;

        private NavMeshAgent _agent;
        private ICrimeAction _pickpocket;
        private ICrimeAction _mugging;
        private ICrimeAction _current;
        private NpcAgent _target;
        private float _repathTimer;

        private NpcAgent[] _allNpcs;

        public event Action<ICrimeAction, CrimeOutcome, NpcAgent> CrimeResolved;
        public event Action<NpcAgent> WitnessReported;
        // 노릴 수 없는 이유(경계 중 등)를 UI에 알린다
        public event Action<string> Rejected;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _pickpocket = _config.CreatePickpocket();
            _mugging = _config.CreateMugging();
            // 목격 판정은 범죄 시점에만 쓰므로 한 번만 모아 둔다
            _allNpcs = FindObjectsByType<NpcAgent>(FindObjectsSortMode.None);
        }

        public void Request(NpcAgent target, CrimeKind kind)
        {
            if (target.IsWary)
            {
                Rejected?.Invoke($"{target.DisplayName}: 경계 중이라 노릴 수 없다");
                return;
            }

            _target = target;
            _current = kind == CrimeKind.Mugging ? _mugging : _pickpocket;
            _repathTimer = 0f;
        }

        public void Cancel()
        {
            _target = null;
        }

        private void Update()
        {
            if (_target == null) return;

            Vector3 offset = _target.transform.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= _current.Range * _current.Range)
            {
                Attempt();
                return;
            }

            _repathTimer -= Time.deltaTime;
            if (_repathTimer > 0f) return;
            _repathTimer = _repathInterval;
            _agent.SetDestination(_target.transform.position);
        }

        private void Attempt()
        {
            NpcAgent target = _target;
            ICrimeAction action = _current;
            _target = null;
            _agent.ResetPath();

            var context = new CrimeContext(
                target.Alertness, target.Money, _needs.GetCrimeModifier(), UnityEngine.Random.value);
            CrimeOutcome outcome = action.Resolve(context);

            // 대상이 실제로 내줄 수 있는 금액만큼만 지갑에 넣는다
            int loot = outcome.Success ? target.TakeMoney(outcome.Loot) : 0;
            _wallet.AddLoot(loot);
            _needs.Add(NeedType.Calm, outcome.CalmDelta);
            if (outcome.EnergyDelta != 0f) _needs.Add(NeedType.Energy, outcome.EnergyDelta);
            _heat.Add(outcome.HeatDelta);

            target.BecomeWary(_config.TargetCooldown);

            CrimeResolved?.Invoke(
                action,
                new CrimeOutcome(outcome.Success, loot, outcome.CalmDelta, outcome.HeatDelta, outcome.EnergyDelta),
                target);

            RollWitnesses(target, outcome.Success);
        }

        // 주변 NPC가 범죄를 봤다면 신고해서 열기가 더 오른다
        private void RollWitnesses(NpcAgent victim, bool success)
        {
            float multiplier = success ? _config.SuccessWitnessFactor : 1f;
            float radiusSqr = _config.WitnessRadius * _config.WitnessRadius;

            for (int i = 0; i < _allNpcs.Length; i++)
            {
                NpcAgent npc = _allNpcs[i];
                if (npc == victim) continue;
                if ((npc.transform.position - transform.position).sqrMagnitude > radiusSqr) continue;
                if (!WitnessLogic.Reports(npc.Alertness, multiplier, UnityEngine.Random.value)) continue;

                _heat.Add(_config.WitnessHeat);
                WitnessReported?.Invoke(npc);
            }
        }
    }
}
