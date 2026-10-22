using System;
using UnityEngine;

namespace CrimeLife.NPC
{
    // 쓸 수 있는 캐릭터 외형 목록. NPC 데이터는 이 목록의 번호만 들고 있다.
    // 모델 프리팹이 없는 환경(외부 에셋 없음)에서는 Prefab이 비어 색 캡슐로 보인다.
    [CreateAssetMenu(fileName = "CharacterCatalog", menuName = "CrimeLife/Character Catalog")]
    public class CharacterCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string Name;
            public GameObject Prefab;
            public Color PlaceholderColor;
        }

        [SerializeField] private Entry[] _entries;

        public int Count => _entries == null ? 0 : _entries.Length;

        // 범위를 벗어난 번호도 안전하게 목록 안으로 돌려서 돌려준다
        public Entry Get(int index)
        {
            if (Count == 0) return new Entry { Name = "", Prefab = null, PlaceholderColor = new Color(0.65f, 0.65f, 0.65f) };
            return _entries[((index % Count) + Count) % Count];
        }
    }
}
