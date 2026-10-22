namespace CrimeLife.NPC
{
    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public static class ScheduleLogic
    {
        // hour 시점에 유효한 항목의 인덱스. 시작 시각 순서로 정렬돼 있지 않아도 된다.
        // 첫 항목보다 이른 시각이면 전날 마지막 항목이 이어지는 것으로 본다.
        public static int GetActiveIndex(ScheduleEntry[] entries, float hour)
        {
            if (entries == null || entries.Length == 0) return -1;

            int active = -1;
            float activeStart = float.MinValue;
            int latest = 0;
            float latestStart = float.MinValue;

            for (int i = 0; i < entries.Length; i++)
            {
                float start = entries[i].StartHour;
                if (start <= hour && start > activeStart)
                {
                    active = i;
                    activeStart = start;
                }
                if (start > latestStart)
                {
                    latest = i;
                    latestStart = start;
                }
            }

            return active >= 0 ? active : latest;
        }
    }
}
