using UnityEngine;

namespace CrimeLife.Build
{
    // 집에 놓인 가구 한 개. 격자 위치와 회전을 들고 있어서 저장과 이동에 쓴다.
    public class PlacedFurniture : MonoBehaviour
    {
        public FurnitureDefinition Definition { get; private set; }
        public Vector2Int Cell { get; private set; }
        public int Rotation { get; private set; }
        public int GridId { get; private set; }

        public void Init(FurnitureDefinition definition, Vector2Int cell, int rotation, int gridId)
        {
            Definition = definition;
            Cell = cell;
            Rotation = rotation;
            GridId = gridId;
        }
    }
}
