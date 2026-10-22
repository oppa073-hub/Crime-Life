using CrimeLife.Needs;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class NeedsModelTests
    {
        private static NeedsModel CreateModel(float start = 80f, float decay = 4f, float critical = 25f)
        {
            var s = new NeedSettings(start, decay, critical);
            return new NeedsModel(new[] { s, s, s }, 0.2f);
        }

        [Test]
        public void Tick_DecreasesAllNeedsByDecayTimesHours()
        {
            var model = CreateModel();
            model.Tick(2f);
            Assert.AreEqual(72f, model.Get(NeedType.Satiety), 0.001f);
            Assert.AreEqual(72f, model.Get(NeedType.Energy), 0.001f);
            Assert.AreEqual(72f, model.Get(NeedType.Calm), 0.001f);
        }

        [Test]
        public void Value_ClampsBetweenZeroAndMax()
        {
            var model = CreateModel();
            model.Add(NeedType.Satiety, 500f);
            Assert.AreEqual(NeedsModel.MaxValue, model.Get(NeedType.Satiety));
            model.Add(NeedType.Satiety, -500f);
            Assert.AreEqual(0f, model.Get(NeedType.Satiety));
        }

        [Test]
        public void BecameCritical_FiresOnlyOnceWhenCrossingThreshold()
        {
            var model = CreateModel(start: 30f, decay: 4f, critical: 25f);
            int fired = 0;
            model.BecameCritical += _ => fired++;

            model.Tick(2f); // 30 -> 22: 임계치 통과
            model.Tick(1f); // 22 -> 18: 이미 위급이므로 재발행 없음

            Assert.AreEqual(3, fired); // 욕구 3개가 각각 한 번씩
        }

        [Test]
        public void CrimeModifier_IsOneWhenNoNeedIsCritical()
        {
            Assert.AreEqual(1f, CreateModel().GetCrimeModifier(), 0.001f);
        }

        [Test]
        public void CrimeModifier_DropsWhenNeedIsCritical()
        {
            var model = CreateModel();
            model.Add(NeedType.Energy, -80f); // 0이 되면 severity 1 -> 0.2 감소
            Assert.AreEqual(0.8f, model.GetCrimeModifier(), 0.001f);
        }

        [Test]
        public void Constructor_ThrowsWhenSettingsCountMismatch()
        {
            Assert.Throws<System.ArgumentException>(() =>
                new NeedsModel(new[] { new NeedSettings(1, 1, 1) }, 0.2f));
        }
    }
}
