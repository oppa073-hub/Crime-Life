using System;
using CrimeLife.Interaction;
using CrimeLife.Needs;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.Crime
{
    // 수배 열기가 높으면 플레이어를 추격하고, 붙잡으면 벌금/스트레스를 물리고 경찰서 앞으로 보낸다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class PoliceOfficer : MonoBehaviour
    {
        [SerializeField] private CrimeConfig _config;
        [SerializeField] private HeatTracker _heat;
        [SerializeField] private Wallet _wallet;
        [SerializeField] private NeedsController _needs;
        [SerializeField] private Transform _player;
        [Tooltip("추격을 포기하면 돌아가는 자리")]
        [SerializeField] private Transform _post;
        [Tooltip("체포된 플레이어가 풀려나는 자리")]
        [SerializeField] private Transform _releasePoint;
        [SerializeField] private float _repathInterval = 0.25f;

        private NavMeshAgent _agent;
        private NavMeshAgent _playerAgent;
        private PlayerCrimeController _playerCrime;
        private PlayerInteractor _playerInteractor;
        private PoliceState _state;
        private float _repathTimer;

        public bool IsChasing => _state == PoliceState.Chasing;

        public event Action<bool> ChasingChanged;
        // (현금 벌금, 압수된 장물)
        public event Action<int, int> PlayerArrested;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = _config.PoliceSpeed;

            _playerAgent = _player.GetComponent<NavMeshAgent>();
            _playerCrime = _player.GetComponent<PlayerCrimeController>();
            _playerInteractor = _player.GetComponent<PlayerInteractor>();
        }

        private void Update()
        {
            PoliceState next = PoliceLogic.Next(_state, _heat.Heat, _config.ChaseHeat, _config.ReleaseHeat);
            if (next != _state) SetState(next);
            if (_state != PoliceState.Chasing) return;

            Vector3 offset = _player.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= _config.CatchRange * _config.CatchRange)
            {
                Arrest();
                return;
            }

            _repathTimer -= Time.deltaTime;
            if (_repathTimer > 0f) return;
            _repathTimer = _repathInterval;
            _agent.SetDestination(_player.position);
        }

        private void SetState(PoliceState state)
        {
            _state = state;
            _repathTimer = 0f;
            if (state == PoliceState.Idle) _agent.SetDestination(_post.position);
            ChasingChanged?.Invoke(state == PoliceState.Chasing);
        }

        private void Arrest()
        {
            int fine = PoliceLogic.CalculateFine(_wallet.Cash, _config.FineFraction);
            int seized = PoliceLogic.CalculateFine(_wallet.Loot, _config.LootSeizeFraction);
            _wallet.Confiscate(fine, seized);
            _needs.Add(NeedType.Calm, -_config.ArrestCalmLoss);
            _heat.Clear();

            // 진행 중이던 이동/범죄/상호작용을 끊고 풀려날 자리로 옮긴다
            _playerCrime.Cancel();
            _playerInteractor.Cancel();
            _playerAgent.ResetPath();
            if (NavMesh.SamplePosition(_releasePoint.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                _playerAgent.Warp(hit.position);

            PlayerArrested?.Invoke(fine, seized);
            SetState(PoliceState.Idle);
        }
    }
}
