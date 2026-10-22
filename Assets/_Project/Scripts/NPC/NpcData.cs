using System;
using UnityEngine;

namespace CrimeLife.NPC
{
    // NPC 한 명의 정체성. 고정 주요 NPC(프로필 에셋)와 랜덤 생성 NPC가 같은 형태를 써서
    // NpcAgent는 어디서 왔는지 몰라도 되고, "아는 사람"으로 저장할 때도 이 데이터를 그대로 저장한다.
    [Serializable]
    public class NpcData
    {
        public string Id;
        public string DisplayName;
        public bool IsMain;

        // 성격 (0~1)
        public float Alertness;
        // 친화력: 높을수록 대화/선물로 친밀도가 잘 오른다
        public float Sociability;
        public string Personality;

        public int Money;
        // 캐릭터 카탈로그의 번호. 에셋이 없는 환경에서도 같은 사람이 같은 색 캡슐로 보인다
        public int AppearanceIndex;
        public float InitialFriendship;

        // 일과표의 "@Home"이 가리키는 집 장소(역할)
        public string HomeId;
        public ScheduleEntry[] Schedule;

        public NpcData Clone() => JsonUtility.FromJson<NpcData>(JsonUtility.ToJson(this));

        // 같은 이름의 장소가 여러 개일 때 이 NPC가 항상 같은 곳으로 가게 하는 고정 값.
        // string.GetHashCode는 실행 환경마다 다를 수 있어서, 저장한 NPC가 다시 불러도 같은 장소로 가도록 직접 계산한다.
        public static int SeedFor(string id)
        {
            unchecked
            {
                int hash = 17;
                foreach (char c in id) hash = hash * 31 + c;
                return hash & int.MaxValue;
            }
        }
    }
}
