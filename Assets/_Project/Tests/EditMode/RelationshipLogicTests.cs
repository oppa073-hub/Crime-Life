using CrimeLife.Core;
using CrimeLife.Social;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class RelationshipLogicTests
    {
        private static readonly RelationshipSettings S = new RelationshipSettings
        {
            AcquaintanceFrom = 20f,
            FriendFrom = 50f,
            CloseFrom = 80f,
            AlertnessReductionAtMax = 0.6f,
            StealLossBase = 10f,
            StealLossPerFriendship = 0.4f,
            CaughtLossBase = 20f,
            CaughtLossPerFriendship = 0.6f,
            GuiltPerFriendship = 0.25f,
        };

        [Test]
        public void Tier_ChangesAtThresholds()
        {
            Assert.AreEqual(RelationshipTier.Stranger, RelationshipLogic.GetTier(0f, S));
            Assert.AreEqual(RelationshipTier.Stranger, RelationshipLogic.GetTier(19.9f, S));
            Assert.AreEqual(RelationshipTier.Acquaintance, RelationshipLogic.GetTier(20f, S));
            Assert.AreEqual(RelationshipTier.Friend, RelationshipLogic.GetTier(50f, S));
            Assert.AreEqual(RelationshipTier.Close, RelationshipLogic.GetTier(80f, S));
            Assert.AreEqual(RelationshipTier.Close, RelationshipLogic.GetTier(100f, S));
        }

        [Test]
        public void EffectiveAlertness_DropsAsFriendshipRises()
        {
            Assert.AreEqual(0.8f, RelationshipLogic.EffectiveAlertness(0.8f, 0f, S), 0.001f);
            Assert.AreEqual(0.56f, RelationshipLogic.EffectiveAlertness(0.8f, 50f, S), 0.001f);
            Assert.AreEqual(0.32f, RelationshipLogic.EffectiveAlertness(0.8f, 100f, S), 0.001f);
        }

        [Test]
        public void EffectiveAlertness_ClampsOutOfRangeFriendship()
        {
            Assert.AreEqual(0.32f, RelationshipLogic.EffectiveAlertness(0.8f, 500f, S), 0.001f);
            Assert.AreEqual(0.8f, RelationshipLogic.EffectiveAlertness(0.8f, -50f, S), 0.001f);
        }

        [Test]
        public void CrimeLoss_GrowsWithFriendship_AndIsBiggerWhenCaught()
        {
            Assert.AreEqual(10f, RelationshipLogic.CrimeFriendshipLoss(0f, true, S), 0.001f);
            Assert.AreEqual(30f, RelationshipLogic.CrimeFriendshipLoss(50f, true, S), 0.001f);
            Assert.AreEqual(20f, RelationshipLogic.CrimeFriendshipLoss(0f, false, S), 0.001f);
            Assert.AreEqual(50f, RelationshipLogic.CrimeFriendshipLoss(50f, false, S), 0.001f);
        }

        [Test]
        public void Guilt_IsZeroForStrangers_AndGrowsWithFriendship()
        {
            Assert.AreEqual(0f, RelationshipLogic.GuiltCalmLoss(0f, S), 0.001f);
            Assert.AreEqual(12.5f, RelationshipLogic.GuiltCalmLoss(50f, S), 0.001f);
        }

        [Test]
        public void Clamp_KeepsFriendshipInRange()
        {
            Assert.AreEqual(0f, RelationshipLogic.Clamp(-5f));
            Assert.AreEqual(100f, RelationshipLogic.Clamp(120f));
            Assert.AreEqual(42f, RelationshipLogic.Clamp(42f));
        }

        [Test]
        public void SaveData_RelationshipsSurviveJsonRoundTrip()
        {
            var data = new SaveData();
            data.Relationships.Add(new RelationshipRecord { NpcId = "Npc_Minsu", Friendship = 37.5f });

            SaveData copy = SaveFile.Deserialize(SaveFile.Serialize(data.Clone()));

            Assert.AreEqual(1, copy.Relationships.Count);
            Assert.AreEqual("Npc_Minsu", copy.Relationships[0].NpcId);
            Assert.AreEqual(37.5f, copy.Relationships[0].Friendship);
        }
    }
}
