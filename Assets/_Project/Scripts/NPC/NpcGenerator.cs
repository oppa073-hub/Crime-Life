using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrimeLife.NPC
{
    [Serializable]
    public struct ScheduleTemplateData
    {
        public string Id;
        // WaypointId에 "@Home"을 쓰면 NPC의 집으로 해석된다
        public ScheduleEntry[] Entries;
    }

    // 랜덤 NPC를 만들 때 쓰는 재료와 범위
    [Serializable]
    public class NpcGeneratorSettings
    {
        public string[] Surnames;
        public string[] GivenNames;
        public ScheduleTemplateData[] Templates;
        public float AlertnessMin = 0.1f;
        public float AlertnessMax = 0.9f;
        public float SociabilityMin = 0.1f;
        public float SociabilityMax = 0.9f;
        public int MoneyMin = 40;
        public int MoneyMax = 400;
    }

    // MonoBehaviour와 분리한 순수 로직. System.Random을 밖에서 받아서 같은 시드면 같은 결과가 나온다.
    public static class NpcGenerator
    {
        public const string HomeToken = "@Home";

        public static NpcData Generate(
            System.Random rng, NpcGeneratorSettings s,
            ICollection<string> usedIds, ICollection<string> usedNames,
            IList<string> homeIds, int appearanceCount)
        {
            string id;
            do { id = "rnd_" + rng.Next().ToString("x8"); } while (usedIds.Contains(id));

            float alertness = Round2(Lerp(s.AlertnessMin, s.AlertnessMax, rng));
            float sociability = Round2(Lerp(s.SociabilityMin, s.SociabilityMax, rng));

            return new NpcData
            {
                Id = id,
                DisplayName = PickName(rng, s, usedNames),
                IsMain = false,
                Alertness = alertness,
                Sociability = sociability,
                Personality = PersonalityLabel(alertness, sociability),
                Money = RoundToTen(s.MoneyMin + rng.Next(Math.Max(1, s.MoneyMax - s.MoneyMin + 1))),
                AppearanceIndex = appearanceCount > 0 ? rng.Next(appearanceCount) : 0,
                InitialFriendship = 0f,
                HomeId = homeIds != null && homeIds.Count > 0 ? homeIds[rng.Next(homeIds.Count)] : "",
                Schedule = PickSchedule(rng, s),
            };
        }

        // 이름이 겹치면 다시 뽑고, 계속 겹치면 번호를 붙여서 구분한다
        public static string PickName(System.Random rng, NpcGeneratorSettings s, ICollection<string> usedNames)
        {
            string name = "";
            for (int attempt = 0; attempt < 30; attempt++)
            {
                name = s.Surnames[rng.Next(s.Surnames.Length)] + s.GivenNames[rng.Next(s.GivenNames.Length)];
                if (!usedNames.Contains(name)) return name;
            }

            int n = 2;
            while (usedNames.Contains(name + n)) n++;
            return name + n;
        }

        public static ScheduleEntry[] PickSchedule(System.Random rng, NpcGeneratorSettings s)
        {
            if (s.Templates == null || s.Templates.Length == 0) return new ScheduleEntry[0];

            ScheduleTemplateData template = s.Templates[rng.Next(s.Templates.Length)];
            // 템플릿 원본을 건드리지 않도록 복사해서 쓴다
            return (ScheduleEntry[])template.Entries.Clone();
        }

        // 성격 수치를 화면에 보여 줄 짧은 말로 바꾼다. 평범한 값은 비워 둔다.
        public static string PersonalityLabel(float alertness, float sociability)
        {
            string a = alertness >= 0.65f ? "의심 많은" : alertness <= 0.35f ? "느긋한" : "";
            string b = sociability >= 0.65f ? "사교적인" : sociability <= 0.35f ? "내성적인" : "";

            if (a != "" && b != "") return a + " · " + b;
            return a + b;
        }

        private static float Lerp(float min, float max, System.Random rng) =>
            min + (max - min) * (float)rng.NextDouble();

        private static float Round2(float value) => Mathf.Round(value * 100f) / 100f;

        private static int RoundToTen(int value) => (value + 5) / 10 * 10;
    }
}
