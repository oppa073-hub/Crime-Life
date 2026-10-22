using UnityEngine;

namespace CrimeLife.NPC
{
    // 랜덤 NPC 생성 재료(이름 풀, 일과표 유형, 수치 범위). 인스펙터에서 바꿔서 분위기를 조절한다.
    // 새 에셋을 만들면 아래 기본값(한글 이름 풀과 일과표 5종)이 채워진다.
    [CreateAssetMenu(fileName = "NpcGeneratorConfig", menuName = "CrimeLife/NPC Generator Config")]
    public class NpcGeneratorConfig : ScriptableObject
    {
        [SerializeField] private NpcGeneratorSettings _settings = CreateDefaultSettings();

        public NpcGeneratorSettings Settings => _settings;

        private static NpcGeneratorSettings CreateDefaultSettings() => new NpcGeneratorSettings
        {
            Surnames = new[]
            {
                "김", "이", "박", "최", "정", "강", "조", "윤", "장", "임",
                "한", "오", "서", "신", "권", "황", "안", "송", "류", "홍",
            },
            GivenNames = new[]
            {
                "민준", "서연", "지우", "하준", "서윤", "도윤", "예준", "지민", "수아", "시우",
                "하윤", "주원", "지호", "다은", "건우", "유진", "은우", "채원", "현우", "소연",
                "준서", "나연", "태양", "보라", "성민", "지은", "재원", "하늘", "동현", "세아",
                "우진", "미나", "기범", "혜진", "상철", "말순", "영수", "정희", "순자", "복동",
            },
            Templates = new[]
            {
                Template("worker", "0:@Home:휴식", "8:Work:출근", "12:Cafe:점심", "13:Work:근무", "18:Plaza:퇴근길", "21:@Home:귀가"),
                Template("homebody", "0:@Home:휴식", "10:Market:장보기", "11:@Home:집안일", "15:Park:산책", "17:@Home:귀가"),
                Template("socialite", "0:@Home:휴식", "10:Cafe:카페", "14:Plaza:산책", "18:Park:모임", "22:@Home:귀가"),
                Template("nightowl", "0:Plaza:밤 산책", "3:@Home:휴식", "13:Cafe:늦은 점심", "17:Market:장보기", "20:Plaza:외출"),
                Template("shopkeeper", "0:@Home:휴식", "7:Market:장사", "17:Plaza:휴식", "20:@Home:귀가"),
            },
        };

        // "시작시:장소:라벨" 문자열로 일과표 한 항목씩 적는다
        private static ScheduleTemplateData Template(string id, params string[] rows)
        {
            var entries = new ScheduleEntry[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                string[] p = rows[i].Split(':');
                entries[i] = new ScheduleEntry { StartHour = float.Parse(p[0]), WaypointId = p[1], Label = p[2] };
            }
            return new ScheduleTemplateData { Id = id, Entries = entries };
        }
    }
}
