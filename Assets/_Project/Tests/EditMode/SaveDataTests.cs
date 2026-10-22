using CrimeLife.Core;
using CrimeLife.Needs;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class SaveDataTests
    {
        [Test]
        public void Json_RoundTripKeepsAllFields()
        {
            var original = new SaveData
            {
                SceneName = "Home",
                Hour = 13.5f,
                Cash = 120,
                Loot = 45,
                Heat = 22.5f,
                Needs = new[] { 80f, 55f, 30f },
            };

            SaveData restored = SaveFile.Deserialize(SaveFile.Serialize(original));

            Assert.AreEqual("Home", restored.SceneName);
            Assert.AreEqual(13.5f, restored.Hour);
            Assert.AreEqual(120, restored.Cash);
            Assert.AreEqual(45, restored.Loot);
            Assert.AreEqual(22.5f, restored.Heat);
            Assert.AreEqual(new[] { 80f, 55f, 30f }, restored.Needs);
        }

        [Test]
        public void NeedsModel_RestoreRoundTripsThroughArray()
        {
            var s = new NeedSettings(80f, 4f, 25f);
            var source = new NeedsModel(new[] { s, s, s }, 0.2f);
            source.Add(NeedType.Energy, -50f);

            var target = new NeedsModel(new[] { s, s, s }, 0.2f);
            target.Restore(source.ToArray());

            Assert.AreEqual(30f, target.Get(NeedType.Energy), 0.001f);
            Assert.AreEqual(80f, target.Get(NeedType.Satiety), 0.001f);
        }

        [Test]
        public void NeedsModel_RestoreIgnoresWrongLength()
        {
            var s = new NeedSettings(80f, 4f, 25f);
            var model = new NeedsModel(new[] { s, s, s }, 0.2f);

            model.Restore(new[] { 1f });
            model.Restore(null);

            Assert.AreEqual(80f, model.Get(NeedType.Satiety), 0.001f);
        }

        [Test]
        public void NeedsModel_RestoreClampsOutOfRangeValues()
        {
            var s = new NeedSettings(80f, 4f, 25f);
            var model = new NeedsModel(new[] { s, s, s }, 0.2f);

            model.Restore(new[] { 500f, -20f, 50f });

            Assert.AreEqual(NeedsModel.MaxValue, model.Get(NeedType.Satiety));
            Assert.AreEqual(0f, model.Get(NeedType.Energy));
        }
    }
}
