namespace CrimeLife.Core
{
    // 저장/복원이 필요한 컴포넌트가 구현한다. 각자 자기 값만 SaveData에 넣고 꺼낸다.
    public interface ISaveable
    {
        void Save(SaveData data);
        void Load(SaveData data);
    }
}
