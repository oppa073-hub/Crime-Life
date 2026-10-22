using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrimeLife.Core
{
    [Serializable]
    public class FurnitureRecord
    {
        public string Id;
        public int X;
        public int Y;
        public int Rotation;
    }

    // 씬을 넘나들 때와 파일 저장에 공통으로 쓰는 플레이어 상태.
    // 값만 담는 단순 데이터라 JsonUtility로 그대로 직렬화한다.
    [Serializable]
    public class RelationshipRecord
    {
        public string NpcId;
        public float Friendship;
    }

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

        // 집을 한 번도 안 열었으면 false: 그때는 집이 기본 배치로 시작한다.
        // (빈 목록과 "저장된 적 없음"을 구분해야 해서 JsonUtility가 못 하는 null 대신 플래그를 쓴다)
        public bool HasFurniture;
        public List<FurnitureRecord> Furniture = new List<FurnitureRecord>();

        // 저장된 NPC만 들어 있고, 없는 NPC는 프로필의 초기 친밀도를 쓴다
        public List<RelationshipRecord> Relationships = new List<RelationshipRecord>();

        public SaveData Clone() => JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(this));
    }
}
