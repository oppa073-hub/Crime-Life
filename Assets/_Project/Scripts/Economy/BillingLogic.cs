using System;
using UnityEngine;

namespace CrimeLife.Economy
{
    [Serializable]
    public struct BillingSettings
    {
        public int Rent;
        public int Utilities;
        // 청구 주기(게임 시간)
        public float PeriodHours;
        // 청구 후 연체로 치기까지 주는 시간
        public float GraceHours;
        // 연체 한 번당 미납액에 붙는 연체료 비율
        public float LateFeeFraction;
        public float OverdueCalmLoss;
        public int SeizuresPerPenalty;
    }

    public class BillingState
    {
        public float NextDueAt;
        public int Unpaid;
        // 음수면 연체 중이 아님
        public float NextPenaltyAt = -1f;
    }

    public struct BillingEvents
    {
        public int BillsIssued;
        public int Penalties;
    }

    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public static class BillingLogic
    {
        // 시간이 now까지 흘렀을 때 발생해야 하는 청구와 연체를 상태에 반영하고, 일어난 일을 돌려준다.
        // 오래 자리를 비워도(시간이 크게 점프해도) 지난 청구/연체를 빠짐없이 처리한다.
        public static BillingEvents Advance(BillingState state, float now, BillingSettings settings)
        {
            var events = new BillingEvents();

            while (settings.PeriodHours > 0f && now >= state.NextDueAt)
            {
                // 미납이 없던 상태에서 처음 청구될 때만 연체 기한이 시작된다
                if (state.Unpaid == 0) state.NextPenaltyAt = state.NextDueAt + settings.GraceHours;

                state.Unpaid += settings.Rent + settings.Utilities;
                state.NextDueAt += settings.PeriodHours;
                events.BillsIssued++;
            }

            while (settings.GraceHours > 0f && state.Unpaid > 0 && state.NextPenaltyAt >= 0f && now >= state.NextPenaltyAt)
            {
                state.Unpaid += Mathf.RoundToInt(state.Unpaid * settings.LateFeeFraction);
                state.NextPenaltyAt += settings.GraceHours;
                events.Penalties++;
            }

            return events;
        }

        public static void Pay(BillingState state)
        {
            state.Unpaid = 0;
            state.NextPenaltyAt = -1f;
        }

        // 미납이 있으면 다음 연체까지, 없으면 다음 청구까지 남은 시간
        public static float HoursLeft(BillingState state, float now) =>
            state.Unpaid > 0 && state.NextPenaltyAt >= 0f
                ? state.NextPenaltyAt - now
                : state.NextDueAt - now;
    }
}
