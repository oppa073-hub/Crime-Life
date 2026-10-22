using UnityEngine;

namespace CrimeLife.Crime
{
    [CreateAssetMenu(fileName = "CrimeConfig", menuName = "CrimeLife/Crime Config")]
    public class CrimeConfig : ScriptableObject
    {
        [Header("소매치기")]
        [SerializeField] private PickpocketSettings _pickpocket = new PickpocketSettings
        {
            Range = 1.8f,
            BaseSuccessChance = 0.8f,
            AlertnessWeight = 0.5f,
            LootFraction = 0.3f,
            StressOnSuccess = 5f,
            StressOnFail = 15f,
            HeatOnSuccess = 5f,
            HeatOnFail = 25f,
        };

        [Header("강도")]
        [SerializeField] private MuggingSettings _mugging = new MuggingSettings
        {
            Range = 1.2f,
            BaseSuccessChance = 0.9f,
            AlertnessWeight = 0.15f,
            LootFraction = 0.8f,
            StressOnSuccess = 20f,
            StressOnFail = 25f,
            HeatOnSuccess = 40f,
            HeatOnFail = 50f,
            EnergyLossOnFail = 25f,
        };

        [Header("수배")]
        [Tooltip("열기가 초당 식는 양")]
        [SerializeField] private float _heatDecayPerSecond = 1.5f;

        [Header("쿨다운/목격")]
        [Tooltip("범죄를 당한 NPC가 경계하는 시간(초)")]
        [SerializeField] private float _targetCooldown = 30f;
        [SerializeField] private float _witnessRadius = 8f;
        [Tooltip("목격자가 신고하면 오르는 열기")]
        [SerializeField] private float _witnessHeat = 15f;
        [Tooltip("범죄가 성공했을 때 목격 확률 배율 (실패하면 1)")]
        [SerializeField, Range(0f, 1f)] private float _successWitnessFactor = 0.5f;

        [Header("경찰")]
        [Tooltip("열기가 이 값 이상이면 경찰이 추격을 시작한다")]
        [SerializeField] private float _chaseHeat = 30f;
        [Tooltip("추격 중 열기가 이 값 아래로 식으면 포기한다")]
        [SerializeField] private float _releaseHeat = 10f;
        [SerializeField] private float _catchRange = 1.5f;
        [SerializeField] private float _policeSpeed = 3.8f;
        [Tooltip("체포되면 지갑에서 빼앗기는 비율")]
        [SerializeField, Range(0f, 1f)] private float _fineFraction = 0.5f;
        [Tooltip("체포되면 장물에서 압수되는 비율. 장물을 들고 다니는 위험을 만든다")]
        [SerializeField, Range(0f, 1f)] private float _lootSeizeFraction = 1f;
        [SerializeField] private float _arrestCalmLoss = 20f;

        public float TargetCooldown => _targetCooldown;
        public float WitnessRadius => _witnessRadius;
        public float WitnessHeat => _witnessHeat;
        public float SuccessWitnessFactor => _successWitnessFactor;
        public float HeatDecayPerSecond => _heatDecayPerSecond;
        public float ChaseHeat => _chaseHeat;
        public float ReleaseHeat => _releaseHeat;
        public float CatchRange => _catchRange;
        public float PoliceSpeed => _policeSpeed;
        public float FineFraction => _fineFraction;
        public float LootSeizeFraction => _lootSeizeFraction;
        public float ArrestCalmLoss => _arrestCalmLoss;

        public ICrimeAction CreatePickpocket() => new PickpocketAction(_pickpocket);
        public ICrimeAction CreateMugging() => new MuggingAction(_mugging);
    }
}
