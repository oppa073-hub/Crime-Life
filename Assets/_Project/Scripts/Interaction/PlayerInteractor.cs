using System;
using CrimeLife.Core;
using CrimeLife.Crime;
using CrimeLife.Economy;
using CrimeLife.Needs;
using UnityEngine;
using UnityEngine.AI;

namespace CrimeLife.Interaction
{
    // 대상 앞까지 걸어간 뒤, 지속시간 동안 욕구를 점진적으로 변화시킨다. (심즈식)
    // 비용이 있으면 현금을 내고, 장물 처분소면 끝날 때 장물을 현금으로 바꿔 준다.
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private NeedsController _needs;
        [SerializeField] private Wallet _wallet;
        [Tooltip("우편함(청구서 납부)용. 청구 시스템이 없는 씬에서는 비워 둔다")]
        [SerializeField] private BillingManager _billing;
        [Tooltip("오브젝트가 NavMesh 밖(장애물 위)이어도 가장 가까운 바닥을 찾는 거리")]
        [SerializeField] private float _sampleDistance = 3f;
        [SerializeField] private float _arriveTolerance = 0.3f;

        private NavMeshAgent _agent;
        private Interactable _target;
        private bool _performing;
        private float _elapsed;

        // 못 하는 이유를 UI에 알린다 (현금 부족, 처분할 장물 없음)
        public event Action<string> Rejected;
        // 장물을 팔아 얻은 현금
        public event Action<int> LootSold;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        public void Request(Interactable target)
        {
            Cancel();
            if (target == null || target.Config == null) return;
            if (!CanUse(target.Config)) return;

            // 가구는 NavMesh 장애물이라 중심점 대신 가장 가까운 바닥으로 접근한다
            if (!NavMesh.SamplePosition(target.transform.position, out NavMeshHit hit, _sampleDistance, NavMesh.AllAreas))
                return;

            _target = target;
            _agent.SetDestination(hit.position);
        }

        public void Cancel()
        {
            _target = null;
            _performing = false;
        }

        private bool CanUse(InteractionConfig config)
        {
            if (config.CashCost > _wallet.Cash)
            {
                Rejected?.Invoke($"{config.DisplayName}: 현금이 부족하다 ({config.CashCost} 필요)");
                return false;
            }
            if (config.LootConversionRate > 0f && _wallet.Loot <= 0)
            {
                Rejected?.Invoke($"{config.DisplayName}: 처분할 장물이 없다");
                return false;
            }
            if (config.PayBills && _billing != null)
            {
                if (_billing.Unpaid <= 0)
                {
                    Rejected?.Invoke($"{config.DisplayName}: 낼 청구서가 없다");
                    return false;
                }
                if (_wallet.Cash < _billing.Unpaid)
                {
                    Rejected?.Invoke($"{config.DisplayName}: 현금이 부족하다 ({_billing.Unpaid} 필요)");
                    return false;
                }
            }
            return true;
        }

        private void Update()
        {
            if (_target == null) return;

            if (!_performing)
            {
                if (_agent.pathPending) return;
                if (_agent.remainingDistance > _agent.stoppingDistance + _arriveTolerance) return;

                // 걸어오는 사이 현금이 줄었을 수 있어 도착했을 때 다시 확인하고 낸다
                if (!CanUse(_target.Config) || !_wallet.TrySpend(_target.Config.CashCost))
                {
                    Cancel();
                    return;
                }

                _performing = true;
                _elapsed = 0f;
                _agent.ResetPath();
                return;
            }

            InteractionConfig config = _target.Config;
            float step = Mathf.Min(Time.deltaTime, config.Duration - _elapsed);
            _elapsed += step;

            NeedEffect[] effects = config.Effects;
            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                    _needs.Add(effects[i].Type, effects[i].TotalAmount * step / config.Duration);
            }

            if (_elapsed < config.Duration) return;

            if (config.LootConversionRate > 0f)
                LootSold?.Invoke(_wallet.ConvertAllLoot(config.LootConversionRate));

            if (config.PayBills && _billing != null) _billing.TryPay(_wallet);

            string scene = config.LoadScene;
            string spawnId = config.SpawnId;
            Cancel();
            if (!string.IsNullOrEmpty(scene)) GameSession.Instance.GoTo(scene, spawnId);
        }
    }
}
