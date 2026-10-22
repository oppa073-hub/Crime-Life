using UnityEngine;

namespace CrimeLife.Economy
{
    [CreateAssetMenu(fileName = "LivingCostConfig", menuName = "CrimeLife/Living Cost Config")]
    public class LivingCostConfig : ScriptableObject
    {
        [SerializeField] private BillingSettings _settings = new BillingSettings
        {
            Rent = 80,
            Utilities = 20,
            PeriodHours = 48f,
            GraceHours = 24f,
            LateFeeFraction = 0.2f,
            OverdueCalmLoss = 30f,
            SeizuresPerPenalty = 1,
        };

        public BillingSettings Settings => _settings;
    }
}
