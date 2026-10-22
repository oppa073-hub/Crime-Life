using CrimeLife.Crime;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class PickpocketActionTests
    {
        private static PickpocketAction Create() => new PickpocketAction(new PickpocketSettings
        {
            Range = 1.8f,
            BaseSuccessChance = 0.8f,
            AlertnessWeight = 0.5f,
            LootFraction = 0.3f,
            StressOnSuccess = 5f,
            StressOnFail = 15f,
            HeatOnSuccess = 5f,
            HeatOnFail = 25f,
        });

        [Test]
        public void SuccessChance_DropsWithAlertness()
        {
            var action = Create();
            Assert.AreEqual(0.8f, action.CalculateSuccessChance(0f, 1f), 0.001f);
            Assert.AreEqual(0.6f, action.CalculateSuccessChance(0.4f, 1f), 0.001f);
        }

        [Test]
        public void SuccessChance_DropsWithBadNeeds()
        {
            var action = Create();
            Assert.AreEqual(0.4f, action.CalculateSuccessChance(0f, 0.5f), 0.001f);
        }

        [Test]
        public void SuccessChance_ClampsToZero()
        {
            Assert.AreEqual(0f, Create().CalculateSuccessChance(1f, 0.1f), 0.001f);
        }

        [Test]
        public void Success_WhenRollBelowChance_TakesFractionOfMoney()
        {
            var outcome = Create().Resolve(new CrimeContext(0.4f, 150, 1f, 0.1f));
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(45, outcome.Loot);
            Assert.AreEqual(-5f, outcome.CalmDelta);
            Assert.AreEqual(5f, outcome.HeatDelta);
        }

        [Test]
        public void Fail_WhenRollAboveChance_PenalizesButGivesNoLoot()
        {
            var outcome = Create().Resolve(new CrimeContext(0.4f, 150, 1f, 0.9f));
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(0, outcome.Loot);
            Assert.AreEqual(-15f, outcome.CalmDelta);
            Assert.AreEqual(25f, outcome.HeatDelta);
        }

        [Test]
        public void Fail_WhenTargetHasNoMoney_EvenWithLuckyRoll()
        {
            Assert.IsFalse(Create().Resolve(new CrimeContext(0f, 0, 1f, 0f)).Success);
        }

        [Test]
        public void Loot_IsAtLeastOneAndNeverMoreThanTargetMoney()
        {
            var outcome = Create().Resolve(new CrimeContext(0f, 2, 1f, 0f));
            Assert.AreEqual(1, outcome.Loot);
        }
    }
}
