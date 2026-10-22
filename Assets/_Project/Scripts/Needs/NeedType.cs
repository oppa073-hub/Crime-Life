namespace CrimeLife.Needs
{
    // 세 욕구 모두 "충족도"(100 = 좋음, 0 = 최악)로 통일해서 계산 분기를 없앤다.
    // 스트레스도 반대로 뒤집은 평온(Calm)으로 다룬다.
    public enum NeedType
    {
        Satiety = 0,
        Energy = 1,
        Calm = 2,
    }
}
