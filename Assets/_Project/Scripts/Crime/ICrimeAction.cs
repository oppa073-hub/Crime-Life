namespace CrimeLife.Crime
{
    public enum CrimeKind
    {
        Pickpocket,
        Mugging,
    }

    // 범죄 판정에 필요한 입력. 새 범죄가 늘어도 같은 입력으로 판정할 수 있게 묶어 둔다.
    public readonly struct CrimeContext
    {
        public readonly float TargetAlertness;
        public readonly int TargetMoney;
        // 생활 → 범죄 훅: NeedsModel.GetCrimeModifier() 값 (0~1)
        public readonly float NeedsModifier;
        // 0~1 난수. 테스트에서 결과를 고정하려고 밖에서 받는다
        public readonly float Roll;

        public CrimeContext(float targetAlertness, int targetMoney, float needsModifier, float roll)
        {
            TargetAlertness = targetAlertness;
            TargetMoney = targetMoney;
            NeedsModifier = needsModifier;
            Roll = roll;
        }
    }

    // 범죄 → 생활 훅: 결과를 욕구(평온/에너지), 수배(열기), 지갑에 반영하는 쪽에서 읽는다
    public readonly struct CrimeOutcome
    {
        public readonly bool Success;
        public readonly int Loot;
        // 음수면 평온이 줄어든다(스트레스 증가)
        public readonly float CalmDelta;
        public readonly float HeatDelta;
        // 음수면 에너지가 줄어든다(부상)
        public readonly float EnergyDelta;

        public CrimeOutcome(bool success, int loot, float calmDelta, float heatDelta, float energyDelta = 0f)
        {
            Success = success;
            Loot = loot;
            CalmDelta = calmDelta;
            HeatDelta = heatDelta;
            EnergyDelta = energyDelta;
        }
    }

    public interface ICrimeAction
    {
        string DisplayName { get; }
        // 이 거리 안에 들어와야 시도할 수 있다
        float Range { get; }
        CrimeOutcome Resolve(CrimeContext context);
    }
}
