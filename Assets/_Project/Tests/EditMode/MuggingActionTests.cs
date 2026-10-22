using CrimeLife.Crime;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class MuggingActionTests
    {
        private static MuggingAction Create() => new MuggingAction(new MuggingSettings
        {
            Range = 1.2f,
            BaseSuccessChance = 0.9f,
            AlertnessWeight = 0.15f,
            LootFraction = 0.8f,
            StressOnSuccess = 20f,
            StressOnFail = 25f,
            HeatOnSuccess = 40f,
            HeatOnFail = 50f,
            EnergyLossOnFail = 25f,
        });

        [Test]
        public void SuccessChance_IsLessSensitiveToAlertnessThanPickpocket()
        {
            // 경계심 0.9여도 0.9 - 0.135 = 0.765
            Assert.AreEqual(0.765f, Create().CalculateSuccessChance(0.9f, 1f), 0.001f);
        }

        [Test]
        public void Success_TakesMostOfTheMoney_AndRaisesHeatALot()
        {
            var outcome = Create().Resolve(new CrimeContext(0.2f, 100, 1f, 0.1f));
            Assert.IsTrue(outcome.Success);
            Assert.AreEqual(80, outcome.Loot);
            Assert.AreEqual(40f, outcome.HeatDelta);
            Assert.AreEqual(0f, outcome.EnergyDelta);
        }

        [Test]
        public void Fail_CostsEnergyAsInjury()
        {
            var outcome = Create().Resolve(new CrimeContext(0.2f, 100, 1f, 0.99f));
            Assert.IsFalse(outcome.Success);
            Assert.AreEqual(0, outcome.Loot);
            Assert.AreEqual(-25f, outcome.EnergyDelta);
            Assert.AreEqual(50f, outcome.HeatDelta);
        }

        [Test]
        public void Fail_WhenTargetHasNoMoney()
        {
            Assert.IsFalse(Create().Resolve(new CrimeContext(0f, 0, 1f, 0f)).Success);
        }

        [Test]
        public void BadNeeds_LowerSuccessChance()
        {
            Assert.AreEqual(0.45f, Create().CalculateSuccessChance(0f, 0.5f), 0.001f);
        }
    }
}
