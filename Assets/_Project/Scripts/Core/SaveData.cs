using System;

namespace CrimeLife.Core
{
    // 씬을 넘나들 때와 파일 저장에 공통으로 쓰는 플레이어 상태.
    // 값만 담는 단순 데이터라 JsonUtility로 그대로 직렬화한다.
    [Serializable]
    public class SaveData
    {
        public string SceneName;
        public float Hour;
        public int Cash;
        public int Loot;
        public float Heat;
        // NeedType 순서(포만감, 에너지, 평온)
        public float[] Needs;
    }
}
