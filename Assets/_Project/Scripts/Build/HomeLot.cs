using System;
using System.Collections.Generic;
using CrimeLife.Core;
using UnityEngine;

namespace CrimeLife.Build
{
    // 집 부지: 격자 위에 가구를 놓고 지우고, 배치를 저장/복원한다.
    public class HomeLot : MonoBehaviour, ISaveable
    {
        [Serializable]
        public struct DefaultPlacement
        {
            public FurnitureDefinition Definition;
            public Vector2Int Cell;
            [Range(0, 3)] public int Rotation;
        }

        [SerializeField] private FurnitureCatalog _catalog;
        [Tooltip("부지 중앙의 월드 좌표 (바닥 높이)")]
        [SerializeField] private Transform _center;
        [SerializeField] private int _width = 14;
        [SerializeField] private int _depth = 14;
        [SerializeField] private float _cellSize = 1f;
        [Tooltip("문 앞처럼 가구를 놓을 수 없는 칸")]
        [SerializeField] private RectInt _reserved;
        [Tooltip("저장된 배치가 없을 때(처음 입장) 놓이는 가구")]
        [SerializeField] private DefaultPlacement[] _defaults;

        private LotGrid _grid;
        private readonly List<PlacedFurniture> _placed = new List<PlacedFurniture>();
        private bool _restored;

        public float CellSize => _cellSize;

        private void Awake()
        {
            _grid = CreateGrid();
        }

        private void Start()
        {
            // 저장된 배치가 이미 복원됐다면(Load) 기본 배치는 건너뛴다
            if (_restored) return;

            foreach (DefaultPlacement item in _defaults)
                Place(item.Definition, item.Cell, item.Rotation);
        }

        public bool CanPlace(FurnitureDefinition definition, Vector2Int cell, int rotation) =>
            _grid.CanPlace(cell, definition.Size, rotation);

        public PlacedFurniture Place(FurnitureDefinition definition, Vector2Int cell, int rotation)
        {
            int gridId = _grid.Place(cell, definition.Size, rotation);
            if (gridId == 0) return null;

            Vector2Int size = LotGrid.RotatedSize(definition.Size, rotation);
            GameObject instance = Instantiate(
                definition.Prefab, GetCenter(cell, size), Quaternion.Euler(0f, rotation * 90f, 0f), transform);

            var placed = instance.GetComponent<PlacedFurniture>();
            placed.Init(definition, cell, rotation, gridId);
            _placed.Add(placed);
            return placed;
        }

        public void Remove(PlacedFurniture furniture)
        {
            _grid.Remove(furniture.GridId);
            _placed.Remove(furniture);
            Destroy(furniture.gameObject);
        }

        // 바닥 위 좌표에서, 가구 발자국의 중심이 그 점에 오도록 하는 기준 칸(왼쪽 아래)
        public Vector2Int WorldToCell(Vector3 world, Vector2Int rotatedSize)
        {
            Vector3 min = GetMinCorner();
            float x = (world.x - min.x) / _cellSize - rotatedSize.x * 0.5f;
            float z = (world.z - min.z) / _cellSize - rotatedSize.y * 0.5f;
            return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
        }

        public Vector3 GetCenter(Vector2Int cell, Vector2Int rotatedSize)
        {
            Vector3 min = GetMinCorner();
            return new Vector3(
                min.x + (cell.x + rotatedSize.x * 0.5f) * _cellSize,
                min.y,
                min.z + (cell.y + rotatedSize.y * 0.5f) * _cellSize);
        }

        // 가구 발자국 안에 월드 좌표가 들어가는지 (플레이어 위에 놓지 못하게 막는 데 쓴다)
        public bool FootprintContains(Vector2Int cell, Vector2Int rotatedSize, Vector3 world)
        {
            Vector3 min = GetMinCorner();
            float x = (world.x - min.x) / _cellSize;
            float z = (world.z - min.z) / _cellSize;
            return x >= cell.x && x < cell.x + rotatedSize.x && z >= cell.y && z < cell.y + rotatedSize.y;
        }

        public void Save(SaveData data)
        {
            data.HasFurniture = true;
            data.Furniture = new List<FurnitureRecord>();
            foreach (PlacedFurniture furniture in _placed)
            {
                data.Furniture.Add(new FurnitureRecord
                {
                    Id = furniture.Definition.Id,
                    X = furniture.Cell.x,
                    Y = furniture.Cell.y,
                    Rotation = furniture.Rotation,
                });
            }
        }

        public void Load(SaveData data)
        {
            if (!data.HasFurniture) return;

            foreach (PlacedFurniture furniture in _placed)
                Destroy(furniture.gameObject);
            _placed.Clear();
            _grid = CreateGrid();

            foreach (FurnitureRecord record in data.Furniture)
            {
                FurnitureDefinition definition = _catalog.Find(record.Id);
                if (definition == null) continue; // 카탈로그에서 빠진 가구는 건너뛴다
                Place(definition, new Vector2Int(record.X, record.Y), record.Rotation);
            }
            _restored = true;
        }

        private LotGrid CreateGrid()
        {
            var grid = new LotGrid(_width, _depth);
            if (_reserved.width > 0 && _reserved.height > 0) grid.Block(_reserved);
            return grid;
        }

        private Vector3 GetMinCorner() =>
            _center.position - new Vector3(_width * _cellSize * 0.5f, 0f, _depth * _cellSize * 0.5f);

        private void OnDrawGizmosSelected()
        {
            if (_center == null) return;

            Vector3 min = GetMinCorner();
            Gizmos.color = new Color(1f, 1f, 1f, 0.35f);
            for (int x = 0; x <= _width; x++)
                Gizmos.DrawLine(min + new Vector3(x * _cellSize, 0.02f, 0f), min + new Vector3(x * _cellSize, 0.02f, _depth * _cellSize));
            for (int z = 0; z <= _depth; z++)
                Gizmos.DrawLine(min + new Vector3(0f, 0.02f, z * _cellSize), min + new Vector3(_width * _cellSize, 0.02f, z * _cellSize));

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            Gizmos.DrawCube(
                min + new Vector3((_reserved.x + _reserved.width * 0.5f) * _cellSize, 0.03f, (_reserved.y + _reserved.height * 0.5f) * _cellSize),
                new Vector3(_reserved.width * _cellSize, 0.02f, _reserved.height * _cellSize));
        }
    }
}
