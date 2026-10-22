using UnityEngine;

namespace CrimeLife.Build
{
    [CreateAssetMenu(fileName = "Furniture", menuName = "CrimeLife/Furniture Definition")]
    public class FurnitureDefinition : ScriptableObject
    {
        [Tooltip("저장 파일에서 가구를 가리키는 이름. 한 번 정하면 바꾸지 않는다")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName = "Furniture";
        [Tooltip("회전 0도 기준 차지하는 칸 수 (가로 x, 세로 z)")]
        [SerializeField] private Vector2Int _size = Vector2Int.one;
        [SerializeField, Min(0)] private int _price = 100;
        [Tooltip("프리팹 루트는 바닥 중앙에 둔다. 모델만 갈아 끼우면 에셋 교체가 끝난다")]
        [SerializeField] private GameObject _prefab;

        public string Id => string.IsNullOrEmpty(_id) ? name : _id;
        public string DisplayName => _displayName;
        public Vector2Int Size => _size;
        public int Price => _price;
        public GameObject Prefab => _prefab;
    }
}
