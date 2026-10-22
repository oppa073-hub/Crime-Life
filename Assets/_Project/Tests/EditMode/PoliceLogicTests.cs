using CrimeLife.Crime;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class PoliceLogicTests
    {
        [Test]
        public void Idle_StartsChasing_WhenHeatReachesThreshold()
        {
            Assert.AreEqual(PoliceState.Idle, PoliceLogic.Next(PoliceState.Idle, 29f, 30f, 10f));
            Assert.AreEqual(PoliceState.Chasing, PoliceLogic.Next(PoliceState.Idle, 30f, 30f, 10f));
        }

        [Test]
        public void Chasing_KeepsChasing_UntilHeatDropsBelowRelease()
        {
            // 추격 시작 기준(30) 아래여도 포기 기준(10) 위면 계속 추격한다
            Assert.AreEqual(PoliceState.Chasing, PoliceLogic.Next(PoliceState.Chasing, 20f, 30f, 10f));
            Assert.AreEqual(PoliceState.Chasing, PoliceLogic.Next(PoliceState.Chasing, 10f, 30f, 10f));
            Assert.AreEqual(PoliceState.Idle, PoliceLogic.Next(PoliceState.Chasing, 9.9f, 30f, 10f));
        }

        [Test]
        public void Fine_IsFractionOfMoney_AndNeverExceedsIt()
        {
            Assert.AreEqual(50, PoliceLogic.CalculateFine(100, 0.5f));
            Assert.AreEqual(0, PoliceLogic.CalculateFine(0, 0.5f));
            Assert.AreEqual(10, PoliceLogic.CalculateFine(10, 5f));
        }
    }
}
