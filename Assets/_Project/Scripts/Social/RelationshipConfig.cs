using UnityEngine;

namespace CrimeLife.Social
{
    [CreateAssetMenu(fileName = "RelationshipConfig", menuName = "CrimeLife/Relationship Config")]
    public class RelationshipConfig : ScriptableObject
    {
        [SerializeField] private RelationshipSettings _settings = new RelationshipSettings
        {
            AcquaintanceFrom = 20f,
            FriendFrom = 50f,
            CloseFrom = 80f,
            ChatGain = 5f,
            GiftGain = 15f,
            GiftCost = 30,
            SocialCooldown = 15f,
            SocialRange = 2f,
            AlertnessReductionAtMax = 0.6f,
            StealLossBase = 10f,
            StealLossPerFriendship = 0.4f,
            CaughtLossBase = 20f,
            CaughtLossPerFriendship = 0.6f,
            GuiltPerFriendship = 0.25f,
        };

        public RelationshipSettings Settings => _settings;
    }
}
