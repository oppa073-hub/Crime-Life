using CrimeLife.Crime;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class WitnessLogicTests
    {
        [Test]
        public void Reports_WhenRollBelowAlertnessTimesMultiplier()
        {
            Assert.IsTrue(WitnessLogic.Reports(0.8f, 1f, 0.5f));
            Assert.IsFalse(WitnessLogic.Reports(0.8f, 1f, 0.9f));
        }

        [Test]
        public void SuccessfulCrime_IsHarderToWitness()
        {
            // 같은 경계심/난수라도 성공(배율 0.5)이면 신고하지 않을 수 있다
            Assert.IsTrue(WitnessLogic.Reports(0.8f, 1f, 0.5f));
            Assert.IsFalse(WitnessLogic.Reports(0.8f, 0.5f, 0.5f));
        }

        [Test]
        public void ZeroAlertness_NeverReports()
        {
            Assert.IsFalse(WitnessLogic.Reports(0f, 1f, 0f));
        }

        [Test]
        public void Chance_IsClampedToOne()
        {
            Assert.IsTrue(WitnessLogic.Reports(0.9f, 5f, 0.999f));
        }
    }
}
