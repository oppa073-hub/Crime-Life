using System;
using System.Collections.Generic;
using CrimeLife.Needs;
using CrimeLife.NPC;
using CrimeLife.Social;
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
        [Tooltip("관계 시스템. 집 씬처럼 NPC가 없는 곳에서는 비워 둔다")]
        [SerializeField] private RelationshipBook _relationships;
        [SerializeField] private RelationshipConfig _relationshipConfig;

        private readonly NpcChaser _chaser = new NpcChaser();
        private NavMeshAgent _agent;
        private ICrimeAction _pickpocket;
        private ICrimeAction _mugging;
        private ICrimeAction _current;

        [Tooltip("도시의 사람들. 목격자를 찾는 데 쓴다. 집 씬처럼 NPC가 없는 곳에서는 비워 둔다")]
        [SerializeField] private PopulationManager _population;

        public event Action<ICrimeAction, CrimeOutcome, NpcAgent> CrimeResolved;
        public event Action<NpcAgent> WitnessReported;
        // 노릴 수 없는 이유(경계 중 등)를 UI에 알린다
        public event Action<string> Rejected;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _pickpocket = _config.CreatePickpocket();
            _mugging = _config.CreateMugging();
        }

        public void Request(NpcAgent target, CrimeKind kind)
        {
            if (target.IsWary)
            {
                Rejected?.Invoke($"{target.DisplayName}: 경계 중이라 노릴 수 없다");
                return;
            }

            _current = kind == CrimeKind.Mugging ? _mugging : _pickpocket;
            _chaser.Begin(target, _current.Range);
        }

        public void Cancel()
        {
            _chaser.Cancel();
        }

        private void Update()
        {
            if (!_chaser.Active) return;

            NpcAgent reached = _chaser.Tick(transform, _agent, Time.deltaTime);
            if (reached != null) Attempt(reached);
        }

        private float Friendship(NpcAgent npc) => _relationships != null ? _relationships.Get(npc) : 0f;

        private float EffectiveAlertness(NpcAgent npc) =>
            _relationships != null
                ? RelationshipLogic.EffectiveAlertness(npc.Alertness, Friendship(npc), _relationshipConfig.Settings)
                : npc.Alertness;

        private void Attempt(NpcAgent target)
        {
            ICrimeAction action = _current;

            // 생활(관계) → 범죄: 친한 사람은 경계가 풀려 있다
            var context = new CrimeContext(
                EffectiveAlertness(target), target.Money, _needs.GetCrimeModifier(), UnityEngine.Random.value);
            CrimeOutcome outcome = action.Resolve(context);

            // 대상이 실제로 내줄 수 있는 금액만큼만 지갑에 넣는다
            int loot = outcome.Success ? target.TakeMoney(outcome.Loot) : 0;
            _wallet.AddLoot(loot);
            _needs.Add(NeedType.Calm, outcome.CalmDelta);
            if (outcome.EnergyDelta != 0f) _needs.Add(NeedType.Energy, outcome.EnergyDelta);
            _heat.Add(outcome.HeatDelta);
            target.BecomeWary(_config.TargetCooldown);

            // 범죄 → 관계: 친한 사람을 털면 관계가 깨지고 죄책감이 든다
            if (_relationships != null) ApplyRelationshipConsequences(target, outcome.Success);

            CrimeResolved?.Invoke(
                action,
                new CrimeOutcome(outcome.Success, loot, outcome.CalmDelta, outcome.HeatDelta, outcome.EnergyDelta),
                target);

            RollWitnesses(target, outcome.Success);
        }

        private void ApplyRelationshipConsequences(NpcAgent target, bool success)
        {
            RelationshipSettings settings = _relationshipConfig.Settings;
            float friendship = _relationships.Get(target);

            _needs.Add(NeedType.Calm, -RelationshipLogic.GuiltCalmLoss(friendship, settings));
            _relationships.Add(target, -RelationshipLogic.CrimeFriendshipLoss(friendship, success, settings));
        }

        // 주변 NPC가 범죄를 봤다면 신고해서 열기가 더 오른다
        private void RollWitnesses(NpcAgent victim, bool success)
        {
            if (_population == null) return;

            float multiplier = success ? _config.SuccessWitnessFactor : 1f;
            float radiusSqr = _config.WitnessRadius * _config.WitnessRadius;

            // NPC가 계속 오가므로 범죄 시점의 현재 목록을 쓴다
            IReadOnlyList<NpcAgent> npcs = _population.Active;
            for (int i = 0; i < npcs.Count; i++)
            {
                NpcAgent npc = npcs[i];
                if (npc == null || npc == victim) continue;
                if ((npc.transform.position - transform.position).sqrMagnitude > radiusSqr) continue;
                // 나와 친한 목격자는 신고를 덜 한다
                if (!WitnessLogic.Reports(EffectiveAlertness(npc), multiplier, UnityEngine.Random.value)) continue;

                _heat.Add(_config.WitnessHeat);
                WitnessReported?.Invoke(npc);
            }
        }
    }
}
