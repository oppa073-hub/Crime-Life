using System.Collections.Generic;
using CrimeLife.Core;
using CrimeLife.NPC;
using CrimeLife.Social;
using NUnit.Framework;

namespace CrimeLife.Tests
{
    public class PopulationTests
    {
        private static NpcGeneratorSettings Settings() => new NpcGeneratorSettings
        {
            Surnames = new[] { "김", "이" },
            GivenNames = new[] { "민준", "서연", "지우" },
            Templates = new[]
            {
                new ScheduleTemplateData
                {
                    Id = "t",
                    Entries = new[]
                    {
                        new ScheduleEntry { StartHour = 0f, WaypointId = "@Home", Label = "휴식" },
                        new ScheduleEntry { StartHour = 9f, WaypointId = "Work", Label = "출근" },
                    },
                },
            },
        };

        private static NpcData Make(int seed, ISet<string> ids = null, ISet<string> names = null) =>
            NpcGenerator.Generate(new System.Random(seed), Settings(), ids ?? new HashSet<string>(),
                names ?? new HashSet<string>(), new List<string> { "Home", "Home2" }, 10);

        // ---------- 생성 ----------

        [Test]
        public void Generate_IsDeterministicForTheSameSeed()
        {
            NpcData a = Make(7);
            NpcData b = Make(7);

            Assert.AreEqual(a.Id, b.Id);
            Assert.AreEqual(a.DisplayName, b.DisplayName);
            Assert.AreEqual(a.Alertness, b.Alertness);
            Assert.AreEqual(a.Money, b.Money);
            Assert.AreEqual(a.AppearanceIndex, b.AppearanceIndex);
        }

        [Test]
        public void Generate_StaysWithinConfiguredRanges()
        {
            var s = Settings();
            var rng = new System.Random(1);
            for (int i = 0; i < 200; i++)
            {
                NpcData d = NpcGenerator.Generate(rng, s, new HashSet<string>(), new HashSet<string>(), new List<string> { "Home" }, 5);

                Assert.GreaterOrEqual(d.Alertness, s.AlertnessMin);
                Assert.LessOrEqual(d.Alertness, s.AlertnessMax);
                Assert.GreaterOrEqual(d.Sociability, s.SociabilityMin);
                Assert.LessOrEqual(d.Sociability, s.SociabilityMax);
                Assert.GreaterOrEqual(d.Money, s.MoneyMin - 5);
                Assert.LessOrEqual(d.Money, s.MoneyMax + 5);
                Assert.AreEqual(0, d.Money % 10);
                Assert.Less(d.AppearanceIndex, 5);
                Assert.IsFalse(d.IsMain);
            }
        }

        [Test]
        public void Generate_AvoidsUsedNamesAndIds()
        {
            var names = new HashSet<string>();
            var ids = new HashSet<string>();
            var rng = new System.Random(3);
            // 이름 조합이 6개뿐이라 7번째부터는 번호가 붙어도 모두 서로 달라야 한다
            for (int i = 0; i < 12; i++)
            {
                NpcData d = NpcGenerator.Generate(rng, Settings(), ids, names, new List<string> { "Home" }, 3);
                Assert.IsTrue(names.Add(d.DisplayName), "이름 중복: " + d.DisplayName);
                Assert.IsTrue(ids.Add(d.Id), "Id 중복: " + d.Id);
            }
        }

        [Test]
        public void Generate_CopiesScheduleTemplateInsteadOfSharingIt()
        {
            var s = Settings();
            NpcData d = NpcGenerator.Generate(new System.Random(2), s, new HashSet<string>(), new HashSet<string>(), new List<string> { "Home" }, 3);
            d.Schedule[0].Label = "바뀜";

            Assert.AreEqual("휴식", s.Templates[0].Entries[0].Label);
        }

        [Test]
        public void Generate_AssignsHomeFromGivenList_AndHandlesNoHomes()
        {
            NpcData withHome = Make(5);
            Assert.That(withHome.HomeId, Is.EqualTo("Home").Or.EqualTo("Home2"));

            NpcData none = NpcGenerator.Generate(new System.Random(5), Settings(), new HashSet<string>(), new HashSet<string>(), new List<string>(), 3);
            Assert.AreEqual("", none.HomeId);
        }

        [Test]
        public void PersonalityLabel_DescribesExtremesOnly()
        {
            Assert.AreEqual("", NpcGenerator.PersonalityLabel(0.5f, 0.5f));
            Assert.AreEqual("의심 많은", NpcGenerator.PersonalityLabel(0.8f, 0.5f));
            Assert.AreEqual("내성적인", NpcGenerator.PersonalityLabel(0.5f, 0.2f));
            Assert.AreEqual("느긋한 · 사교적인", NpcGenerator.PersonalityLabel(0.2f, 0.9f));
        }

        // ---------- 순환 / 아는 사람 ----------

        [Test]
        public void Rotation_SkipsProtectedAndNearbyNpcs()
        {
            var candidates = new List<RotationCandidate>
            {
                new RotationCandidate { Index = 0, DistanceToPlayer = 30f, Protected = false },
                new RotationCandidate { Index = 1, DistanceToPlayer = 5f, Protected = false },
                new RotationCandidate { Index = 2, DistanceToPlayer = 40f, Protected = true },
                new RotationCandidate { Index = 3, DistanceToPlayer = 20f, Protected = false },
            };

            List<int> picked = PopulationLogic.SelectForRotation(candidates, 14f, 5, new System.Random(1));

            CollectionAssert.AreEquivalent(new[] { 0, 3 }, picked);
        }

        [Test]
        public void Rotation_NeverReturnsMoreThanRequested()
        {
            var candidates = new List<RotationCandidate>();
            for (int i = 0; i < 10; i++) candidates.Add(new RotationCandidate { Index = i, DistanceToPlayer = 50f });

            Assert.AreEqual(3, PopulationLogic.SelectForRotation(candidates, 14f, 3, new System.Random(4)).Count);
            Assert.AreEqual(0, PopulationLogic.SelectForRotation(candidates, 14f, 0, new System.Random(4)).Count);
        }

        [Test]
        public void ShouldUseKnown_NeverWhenNoOneIsKnown_AlwaysWhenChanceIsOne()
        {
            Assert.IsFalse(PopulationLogic.ShouldUseKnown(new System.Random(1), 1f, 0));
            Assert.IsTrue(PopulationLogic.ShouldUseKnown(new System.Random(1), 1.1f, 3));
            Assert.IsFalse(PopulationLogic.ShouldUseKnown(new System.Random(1), 0f, 3));
        }

        [Test]
        public void ShouldUseKnown_MatchesTheGivenProbabilityPerPerson()
        {
            var rng = new System.Random(11);
            int hits = 0;
            const int trials = 20000;
            for (int i = 0; i < trials; i++)
            {
                if (PopulationLogic.ShouldUseKnown(rng, 0.06f, 1)) hits++;
            }

            // 한 사람당 약 6%: 아는 사람이 한 명뿐이어도 항상 나타나지 않고 "가끔" 나타난다
            Assert.AreEqual(0.06, hits / (double)trials, 0.01);
        }

        // ---------- 저장 / 안정 값 ----------

        [Test]
        public void KnownNpcs_SurviveSaveDataJsonRoundTrip_IncludingSchedule()
        {
            NpcData npc = Make(9);
            npc.Money = 123;
            var data = new SaveData();
            data.KnownNpcs.Add(npc);

            SaveData copy = SaveFile.Deserialize(SaveFile.Serialize(data.Clone()));

            Assert.AreEqual(1, copy.KnownNpcs.Count);
            NpcData restored = copy.KnownNpcs[0];
            Assert.AreEqual(npc.Id, restored.Id);
            Assert.AreEqual(npc.DisplayName, restored.DisplayName);
            Assert.AreEqual(123, restored.Money);
            Assert.AreEqual(npc.Schedule.Length, restored.Schedule.Length);
            Assert.AreEqual("Work", restored.Schedule[1].WaypointId);
        }

        [Test]
        public void SeedFor_IsStableAndNonNegative()
        {
            Assert.AreEqual(NpcData.SeedFor("rnd_0000abcd"), NpcData.SeedFor("rnd_0000abcd"));
            Assert.AreNotEqual(NpcData.SeedFor("a"), NpcData.SeedFor("b"));
            Assert.GreaterOrEqual(NpcData.SeedFor("아주 긴 이름의 NPC 아이디 12345"), 0);
        }

        [Test]
        public void SocialGain_ScalesWithSociability()
        {
            Assert.AreEqual(5f, RelationshipLogic.SocialGain(10f, 0f), 0.001f);
            Assert.AreEqual(10f, RelationshipLogic.SocialGain(10f, 0.5f), 0.001f);
            Assert.AreEqual(15f, RelationshipLogic.SocialGain(10f, 1f), 0.001f);
            Assert.AreEqual(15f, RelationshipLogic.SocialGain(10f, 3f), 0.001f);
        }
    }
}
