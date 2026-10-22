using System;
using UnityEngine;

namespace CrimeLife.Social
{
    public enum RelationshipTier
    {
        Stranger,
        Acquaintance,
        Friend,
        Close,
    }

    public enum SocialKind
    {
        Chat,
        Gift,
    }

    [Serializable]
    public struct RelationshipSettings
    {
        public float AcquaintanceFrom;
        public float FriendFrom;
        public float CloseFrom;

        public float ChatGain;
        public float GiftGain;
        public int GiftCost;
        // 대화/선물 사이에 기다려야 하는 시간(초)
        public float SocialCooldown;
        public float SocialRange;

        // 친밀도 100일 때 경계심이 줄어드는 비율 (친한 사람은 덜 의심한다)
        public float AlertnessReductionAtMax;

        // 범죄로 잃는 친밀도 = Base + PerFriendship * 현재 친밀도. 들키면(실패) 더 크게 잃는다
        public float StealLossBase;
        public float StealLossPerFriendship;
        public float CaughtLossBase;
        public float CaughtLossPerFriendship;

        // 친한 사람을 털면 평온이 추가로 줄어드는 양 = 친밀도 * 이 값
        public float GuiltPerFriendship;
    }

    // MonoBehaviour와 분리한 순수 로직. EditMode 테스트에서 직접 검증한다.
    public static class RelationshipLogic
    {
        public const float Max = 100f;

        public static float Clamp(float friendship) => Mathf.Clamp(friendship, 0f, Max);

        public static RelationshipTier GetTier(float friendship, RelationshipSettings s)
        {
            if (friendship >= s.CloseFrom) return RelationshipTier.Close;
            if (friendship >= s.FriendFrom) return RelationshipTier.Friend;
            if (friendship >= s.AcquaintanceFrom) return RelationshipTier.Acquaintance;
            return RelationshipTier.Stranger;
        }

        // 생활(관계) → 범죄 훅: 친할수록 경계가 풀려서 털기 쉽고, 목격해도 신고를 덜 한다
        public static float EffectiveAlertness(float alertness, float friendship, RelationshipSettings s) =>
            alertness * (1f - s.AlertnessReductionAtMax * Mathf.Clamp01(friendship / Max));

        // 범죄 → 관계 훅: 친한 사람일수록, 그리고 들켰을수록 관계가 크게 깨진다
        public static float CrimeFriendshipLoss(float friendship, bool success, RelationshipSettings s)
        {
            float baseLoss = success ? s.StealLossBase : s.CaughtLossBase;
            float perFriendship = success ? s.StealLossPerFriendship : s.CaughtLossPerFriendship;
            return Mathf.Max(0f, baseLoss + perFriendship * friendship);
        }

        public static float GuiltCalmLoss(float friendship, RelationshipSettings s) =>
            Mathf.Max(0f, friendship * s.GuiltPerFriendship);
    }
}
