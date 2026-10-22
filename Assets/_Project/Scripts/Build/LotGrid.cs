using System.Collections.Generic;
using UnityEngine;

namespace CrimeLife.Build
{
    // 집 부지의 점유 상태를 관리하는 순수 로직. MonoBehaviour와 분리해 EditMode 테스트로 검증한다.
    public class LotGrid
    {
        private const int Empty = 0;
        private const int Blocked = -1;

        private readonly int[] _cells;
        private readonly Dictionary<int, RectInt> _footprints = new Dictionary<int, RectInt>();
        private int _nextId = 1;

        public int Width { get; }
        public int Depth { get; }

        public LotGrid(int width, int depth)
        {
            Width = width;
            Depth = depth;
            _cells = new int[width * depth];
        }

        // 90도 단위 회전이면 가로/세로가 바뀐다
        public static Vector2Int RotatedSize(Vector2Int size, int rotation) =>
            (rotation & 1) == 0 ? size : new Vector2Int(size.y, size.x);

        public bool InBounds(RectInt rect) =>
            rect.xMin >= 0 && rect.yMin >= 0 && rect.xMax <= Width && rect.yMax <= Depth;

        public bool CanPlace(Vector2Int cell, Vector2Int size, int rotation)
        {
            var rect = new RectInt(cell, RotatedSize(size, rotation));
            if (!InBounds(rect)) return false;

            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                for (int x = rect.xMin; x < rect.xMax; x++)
                {
                    if (_cells[y * Width + x] != Empty) return false;
                }
            }
            return true;
        }

        // 성공하면 1 이상의 id, 놓을 수 없으면 0
        public int Place(Vector2Int cell, Vector2Int size, int rotation)
        {
            if (!CanPlace(cell, size, rotation)) return 0;

            var rect = new RectInt(cell, RotatedSize(size, rotation));
            int id = _nextId++;
            Fill(rect, id);
            _footprints[id] = rect;
            return id;
        }

        public bool Remove(int id)
        {
            if (!_footprints.TryGetValue(id, out RectInt rect)) return false;

            Fill(rect, Empty);
            _footprints.Remove(id);
            return true;
        }

        // 문 앞처럼 가구를 놓으면 안 되는 구역
        public void Block(RectInt rect)
        {
            Fill(new RectInt(
                Mathf.Max(0, rect.xMin), Mathf.Max(0, rect.yMin),
                Mathf.Min(Width, rect.xMax) - Mathf.Max(0, rect.xMin),
                Mathf.Min(Depth, rect.yMax) - Mathf.Max(0, rect.yMin)), Blocked);
        }

        public bool IsOccupied(Vector2Int cell) =>
            cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Depth && _cells[cell.y * Width + cell.x] != Empty;

        private void Fill(RectInt rect, int value)
        {
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                for (int x = rect.xMin; x < rect.xMax; x++)
                    _cells[y * Width + x] = value;
            }
        }
    }
}
