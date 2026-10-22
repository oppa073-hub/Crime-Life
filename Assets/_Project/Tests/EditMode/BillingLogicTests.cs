using CrimeLife.Core;
using CrimeLife.Economy;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class BillingLogicTests
    {
        private static readonly BillingSettings S = new BillingSettings
        {
            Rent = 80,
            Utilities = 20,
            PeriodHours = 48f,
            GraceHours = 24f,
            LateFeeFraction = 0.2f,
            OverdueCalmLoss = 30f,
            SeizuresPerPenalty = 1,
        };

        private static BillingState NewState(float firstDue = 48f) => new BillingState { NextDueAt = firstDue };

        [Test]
        public void NothingHappens_BeforeFirstDue()
        {
            var state = NewState();
            BillingEvents e = BillingLogic.Advance(state, 47.9f, S);

            Assert.AreEqual(0, e.BillsIssued);
            Assert.AreEqual(0, state.Unpaid);
        }

        [Test]
        public void BillIsIssued_AtDueTime_AndGraceStarts()
        {
            var state = NewState();
            BillingEvents e = BillingLogic.Advance(state, 48f, S);

            Assert.AreEqual(1, e.BillsIssued);
            Assert.AreEqual(100, state.Unpaid);
            Assert.AreEqual(72f, state.NextPenaltyAt, 0.001f);
            Assert.AreEqual(96f, state.NextDueAt, 0.001f);
        }

        [Test]
        public void Penalty_AddsLateFee_AfterGracePeriod()
        {
            var state = NewState();
            BillingLogic.Advance(state, 48f, S);
            BillingEvents e = BillingLogic.Advance(state, 72f, S);

            Assert.AreEqual(1, e.Penalties);
            Assert.AreEqual(120, state.Unpaid);
            Assert.AreEqual(96f, state.NextPenaltyAt, 0.001f);
        }

        [Test]
        public void Paying_ClearsDebtAndPenaltyClock_ButKeepsNextBillSchedule()
        {
            var state = NewState();
            BillingLogic.Advance(state, 60f, S);
            BillingLogic.Pay(state);

            Assert.AreEqual(0, state.Unpaid);
            Assert.Less(state.NextPenaltyAt, 0f);
            Assert.AreEqual(96f, state.NextDueAt, 0.001f);
            // 납부했으니 연체 시각이 지나도 아무 일도 없다
            Assert.AreEqual(0, BillingLogic.Advance(state, 90f, S).Penalties);
        }

        [Test]
        public void LongAbsence_ProcessesEveryMissedBillAndPenalty()
        {
            var state = NewState();
            // 48(청구) 96(청구) 144(청구), 연체는 72,96,120,144...
            BillingEvents e = BillingLogic.Advance(state, 150f, S);

            Assert.AreEqual(3, e.BillsIssued);
            Assert.AreEqual(4, e.Penalties);
            Assert.Greater(state.Unpaid, 300);
        }

        [Test]
        public void HoursLeft_CountsToPenaltyWhenOwing_OtherwiseToNextBill()
        {
            var state = NewState();
            Assert.AreEqual(40f, BillingLogic.HoursLeft(state, 8f), 0.001f);

            BillingLogic.Advance(state, 50f, S);
            Assert.AreEqual(22f, BillingLogic.HoursLeft(state, 50f), 0.001f);
        }

        [Test]
        public void ZeroPeriod_DoesNotHang()
        {
            var settings = S;
            settings.PeriodHours = 0f;
            settings.GraceHours = 0f;

            Assert.DoesNotThrow(() => BillingLogic.Advance(NewState(0f), 10f, settings));
        }

        [Test]
        public void SaveData_BillingAndDayFieldsSurviveRoundTrip()
        {
            var data = new SaveData
            {
                Day = 4, BillingInitialized = true, BillingNextDueAt = 144f,
                BillingUnpaid = 120, BillingNextPenaltyAt = 96f, PendingSeizures = 2,
            };

            SaveData copy = SaveFile.Deserialize(SaveFile.Serialize(data.Clone()));

            Assert.AreEqual(4, copy.Day);
            Assert.IsTrue(copy.BillingInitialized);
            Assert.AreEqual(120, copy.BillingUnpaid);
            Assert.AreEqual(96f, copy.BillingNextPenaltyAt);
            Assert.AreEqual(2, copy.PendingSeizures);
        }
    }
}
