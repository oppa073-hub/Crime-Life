using System;
using UnityEngine;

namespace CrimeLife.Crime
{
    [Serializable]
    public struct MuggingSettings
    {
        public float Range;
        public float BaseSuccessChance;
        // 소매치기보다 훨씬 작게 둔다: 눈치채도 힘으로 밀어붙이는 범죄라서
        public float AlertnessWeight;
        public float LootFraction;
        public float StressOnSuccess;
        public float StressOnFail;
        public float HeatOnSuccess;
        public float HeatOnFail;
        // 실패하면 반격당해 다치는 양 (에너지 감소)
        public float EnergyLossOnFail;
    }

    public class MuggingAction : ICrimeAction
    {
        private readonly MuggingSettings _settings;

        public MuggingAction(MuggingSettings settings)
        {
            _settings = settings;
        }

        public string DisplayName => "강도";

        public float Range => _settings.Range;

        public float CalculateSuccessChance(float targetAlertness, float needsModifier) =>
            Mathf.Clamp01(_settings.BaseSuccessChance * needsModifier - targetAlertness * _settings.AlertnessWeight);

        public CrimeOutcome Resolve(CrimeContext context)
        {
            float chance = CalculateSuccessChance(context.TargetAlertness, context.NeedsModifier);

            bool success = context.TargetMoney > 0 && context.Roll < chance;
            if (!success)
            {
                return new CrimeOutcome(
                    false, 0, -_settings.StressOnFail, _settings.HeatOnFail, -_settings.EnergyLossOnFail);
            }

            int loot = Mathf.Clamp(Mathf.RoundToInt(context.TargetMoney * _settings.LootFraction), 1, context.TargetMoney);
            return new CrimeOutcome(true, loot, -_settings.StressOnSuccess, _settings.HeatOnSuccess);
        }
    }
}
