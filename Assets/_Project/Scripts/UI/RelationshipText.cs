using CrimeLife.Social;

namespace CrimeLife.UI
{
    // 관계 단계의 화면 표시용 한글 이름. 로직(RelationshipLogic)은 표시 문자열을 모른다.
    public static class RelationshipText
    {
        public static string TierName(RelationshipTier tier)
        {
            switch (tier)
            {
                case RelationshipTier.Acquaintance: return "아는 사이";
                case RelationshipTier.Friend: return "친구";
                case RelationshipTier.Close: return "절친";
                default: return "낯선 사람";
            }
        }
    }
}
