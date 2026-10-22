using System;
using System.Collections.Generic;

namespace CrimeLife.NPC
{
    public struct RotationCandidate
    {
        // 후보 목록에서의 위치
        public int Index;
        public float DistanceToPlayer;
        // 지금 부르는 중이거나 하는 일이 있어서 내보내면 안 되는 NPC
        public bool Protected;
    }

    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public static class PopulationLogic
    {
        // 순환으로 내보낼 NPC를 최대 count명 고른다.
        // 플레이어 눈앞에서 사라지지 않게 일정 거리 이상 떨어진 NPC만 대상으로 한다.
        public static List<int> SelectForRotation(
            IList<RotationCandidate> candidates, float minDistance, int count, System.Random rng)
        {
            var eligible = new List<int>();
            foreach (RotationCandidate c in candidates)
            {
                if (!c.Protected && c.DistanceToPlayer >= minDistance) eligible.Add(c.Index);
            }

            // 섞은 앞쪽에서 count명을 뽑는다 (Fisher-Yates)
            for (int i = eligible.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int tmp = eligible[i];
                eligible[i] = eligible[j];
                eligible[j] = tmp;
            }

            if (eligible.Count > count) eligible.RemoveRange(count, eligible.Count - count);
            return eligible;
        }

        // 아는 사람 한 명(또는 후보 availableKnown명 중 하나)이 도시에 나타날지 여부. 후보가 없으면 항상 아니다.
        public static bool ShouldUseKnown(System.Random rng, float chance, int availableKnown) =>
            availableKnown > 0 && rng.NextDouble() < chance;
    }
}
