using System;
using UnityEngine;

namespace CrimeLife.Crime
{
    [Serializable]
    public struct PickpocketSettings
    {
        public float Range;
        public float BaseSuccessChance;
        // 상대 경계심 1.0일 때 깎이는 확률
        public float AlertnessWeight;
        // 성공 시 상대 소지금에서 가져가는 비율
        public float LootFraction;
        public float StressOnSuccess;
        public float StressOnFail;
        public float HeatOnSuccess;
        public float HeatOnFail;
    }

    public class PickpocketAction : ICrimeAction
    {
        private readonly PickpocketSettings _settings;

        public PickpocketAction(PickpocketSettings settings)
        {
            _settings = settings;
        }

        public string DisplayName => "소매치기";

        public float Range => _settings.Range;

        public float CalculateSuccessChance(float targetAlertness, float needsModifier) =>
            Mathf.Clamp01(_settings.BaseSuccessChance * needsModifier - targetAlertness * _settings.AlertnessWeight);

        public CrimeOutcome Resolve(CrimeContext context)
        {
            float chance = CalculateSuccessChance(context.TargetAlertness, context.NeedsModifier);

            // 훔칠 돈이 없으면 성공해도 얻는 게 없으므로 실패로 친다
            bool success = context.TargetMoney > 0 && context.Roll < chance;
            if (!success)
                return new CrimeOutcome(false, 0, -_settings.StressOnFail, _settings.HeatOnFail);

            int loot = Mathf.Clamp(Mathf.RoundToInt(context.TargetMoney * _settings.LootFraction), 1, context.TargetMoney);
            return new CrimeOutcome(true, loot, -_settings.StressOnSuccess, _settings.HeatOnSuccess);
        }
    }
}
