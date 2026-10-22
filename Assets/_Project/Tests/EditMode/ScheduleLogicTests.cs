using CrimeLife.NPC;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class ScheduleLogicTests
    {
        private static ScheduleEntry E(float hour) => new ScheduleEntry { StartHour = hour, WaypointId = "w" + hour };

        [Test]
        public void ReturnsMinusOne_WhenNoEntries()
        {
            Assert.AreEqual(-1, ScheduleLogic.GetActiveIndex(null, 10f));
            Assert.AreEqual(-1, ScheduleLogic.GetActiveIndex(new ScheduleEntry[0], 10f));
        }

        [Test]
        public void ReturnsLatestEntryStartedBeforeHour()
        {
            var schedule = new[] { E(0), E(8), E(18) };
            Assert.AreEqual(1, ScheduleLogic.GetActiveIndex(schedule, 12f));
            Assert.AreEqual(2, ScheduleLogic.GetActiveIndex(schedule, 18f)); // 경계는 새 항목 포함
        }

        [Test]
        public void WrapsToLastEntry_WhenBeforeFirstStart()
        {
            var schedule = new[] { E(8), E(22) };
            Assert.AreEqual(1, ScheduleLogic.GetActiveIndex(schedule, 3f));
        }

        [Test]
        public void WorksWithUnsortedEntries()
        {
            var schedule = new[] { E(18), E(0), E(8) };
            Assert.AreEqual(2, ScheduleLogic.GetActiveIndex(schedule, 9f));
            Assert.AreEqual(0, ScheduleLogic.GetActiveIndex(schedule, 20f));
        }
    }
}
