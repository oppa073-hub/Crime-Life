using System;
using CrimeLife.Build;
using CrimeLife.Core;
using CrimeLife.Crime;
using CrimeLife.Needs;
using UnityEngine;

namespace CrimeLife.Economy
{
    // 월세/공과금 청구, 납부, 연체 처리. 집과 도시 양쪽 씬에 있어서 어디서든 시간이 흐르면 청구가 쌓인다.
    public class BillingManager : MonoBehaviour, ISaveable
    {
        [SerializeField] private LivingCostConfig _config;
        [SerializeField] private GameClock _clock;
        [SerializeField] private NeedsController _needs;
        [Tooltip("집 씬에서만 연결한다. 도시에서 생긴 압류는 집에 들어올 때 적용된다")]
        [SerializeField] private HomeLot _lot;

        private BillingState _state = new BillingState();
        private bool _initialized;
        private int _pendingSeizures;
        private int _lastHourKey = -1;

        public int Unpaid => _state.Unpaid;
        public float HoursLeft => BillingLogic.HoursLeft(_state, _clock.TotalHours);

        public event Action Changed;
        public event Action<string> Notice;
        public event Action<int> BillPaid;

        private void Start()
        {
            // 저장된 상태가 복원되지 않았으면(처음 시작) 첫 청구는 한 주기 뒤로 잡는다
            if (!_initialized)
            {
                _state.NextDueAt = _clock.TotalHours + _config.Settings.PeriodHours;
                _initialized = true;
            }

            // 도시에서 밀린 압류를 집에 들어왔을 때 적용한다
            ApplyPendingSeizures();
            Changed?.Invoke();
        }

        private void Update()
        {
            // 게임 시간으로 한 시간이 바뀔 때만 확인한다
            int hourKey = Mathf.FloorToInt(_clock.TotalHours);
            if (hourKey == _lastHourKey) return;
            _lastHourKey = hourKey;

            Evaluate();
        }

        private void Evaluate()
        {
            BillingEvents events = BillingLogic.Advance(_state, _clock.TotalHours, _config.Settings);

            if (events.BillsIssued > 0)
                Notice?.Invoke($"청구서가 도착했다: 미납 {_state.Unpaid} (우편함에서 납부)");

            for (int i = 0; i < events.Penalties; i++)
            {
                // 연체 → 생활: 스트레스를 받고, 가구가 압류된다
                _needs.Add(NeedType.Calm, -_config.Settings.OverdueCalmLoss);
                _pendingSeizures += _config.Settings.SeizuresPerPenalty;
            }
            if (events.Penalties > 0)
            {
                Notice?.Invoke($"연체! 연체료가 붙었다 (미납 {_state.Unpaid})");
                ApplyPendingSeizures();
            }

            Changed?.Invoke();
        }

        private void ApplyPendingSeizures()
        {
            if (_pendingSeizures <= 0 || _lot == null) return;

            var seized = _lot.SeizeMostExpensive(_pendingSeizures);
            _pendingSeizures = 0;
            if (seized.Count > 0) Notice?.Invoke($"연체로 가구가 압류됐다: {string.Join(", ", seized)}");
        }

        // 미납액을 전부 내거나, 현금이 모자라면 아무것도 내지 않는다. 낸 금액을 돌려준다(못 내면 0).
        public int TryPay(Wallet wallet)
        {
            int amount = _state.Unpaid;
            if (amount <= 0 || !wallet.TrySpend(amount)) return 0;

            BillingLogic.Pay(_state);
            BillPaid?.Invoke(amount);
            Changed?.Invoke();
            return amount;
        }

        public void Save(SaveData data)
        {
            data.BillingInitialized = _initialized;
            data.BillingNextDueAt = _state.NextDueAt;
            data.BillingUnpaid = _state.Unpaid;
            data.BillingNextPenaltyAt = _state.NextPenaltyAt;
            data.PendingSeizures = _pendingSeizures;
        }

        public void Load(SaveData data)
        {
            if (!data.BillingInitialized) return;

            _state.NextDueAt = data.BillingNextDueAt;
            _state.Unpaid = data.BillingUnpaid;
            _state.NextPenaltyAt = data.BillingNextPenaltyAt;
            _pendingSeizures = data.PendingSeizures;
            _initialized = true;
            _lastHourKey = -1;
        }
    }
}
