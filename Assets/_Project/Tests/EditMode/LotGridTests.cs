using CrimeLife.Build;
using CrimeLife.Core;
using NUnit.Framework;
using UnityEngine;

namespace CrimeLife.Tests
{
    public class LotGridTests
    {
        [Test]
        public void Place_ReturnsPositiveId_AndOccupiesFootprint()
        {
            var grid = new LotGrid(10, 10);
            int id = grid.Place(new Vector2Int(2, 3), new Vector2Int(2, 1), 0);

            Assert.Greater(id, 0);
            Assert.IsTrue(grid.IsOccupied(new Vector2Int(2, 3)));
            Assert.IsTrue(grid.IsOccupied(new Vector2Int(3, 3)));
            Assert.IsFalse(grid.IsOccupied(new Vector2Int(4, 3)));
            Assert.IsFalse(grid.IsOccupied(new Vector2Int(2, 4)));
        }

        [Test]
        public void Place_FailsWhenOverlapping()
        {
            var grid = new LotGrid(10, 10);
            grid.Place(new Vector2Int(2, 2), new Vector2Int(2, 2), 0);

            Assert.AreEqual(0, grid.Place(new Vector2Int(3, 3), new Vector2Int(2, 2), 0));
            Assert.IsTrue(grid.CanPlace(new Vector2Int(4, 2), new Vector2Int(2, 2), 0));
        }

        [Test]
        public void CanPlace_RejectsOutOfBounds()
        {
            var grid = new LotGrid(5, 5);
            Assert.IsFalse(grid.CanPlace(new Vector2Int(-1, 0), Vector2Int.one, 0));
            Assert.IsFalse(grid.CanPlace(new Vector2Int(4, 4), new Vector2Int(2, 1), 0));
            Assert.IsTrue(grid.CanPlace(new Vector2Int(3, 4), new Vector2Int(2, 1), 0));
        }

        [Test]
        public void Rotation_SwapsFootprintWidthAndDepth()
        {
            var grid = new LotGrid(5, 5);
            // 3x1을 90도 돌리면 1x3: 가로 끝에서는 안 들어가고 세로로는 들어간다
            Assert.IsFalse(grid.CanPlace(new Vector2Int(3, 0), new Vector2Int(3, 1), 0));
            Assert.IsTrue(grid.CanPlace(new Vector2Int(3, 0), new Vector2Int(3, 1), 1));
            Assert.AreEqual(new Vector2Int(1, 3), LotGrid.RotatedSize(new Vector2Int(3, 1), 1));
            Assert.AreEqual(new Vector2Int(3, 1), LotGrid.RotatedSize(new Vector2Int(3, 1), 2));
        }

        [Test]
        public void Remove_FreesTheCells()
        {
            var grid = new LotGrid(5, 5);
            int id = grid.Place(Vector2Int.zero, new Vector2Int(2, 2), 0);

            Assert.IsTrue(grid.Remove(id));
            Assert.IsFalse(grid.IsOccupied(Vector2Int.zero));
            Assert.IsTrue(grid.CanPlace(Vector2Int.zero, new Vector2Int(2, 2), 0));
            Assert.IsFalse(grid.Remove(id));
        }

        [Test]
        public void Block_PreventsPlacementAndSurvivesRemoveOfOthers()
        {
            var grid = new LotGrid(6, 6);
            grid.Block(new RectInt(0, 0, 2, 2));

            Assert.IsFalse(grid.CanPlace(Vector2Int.one, Vector2Int.one, 0));
            Assert.IsTrue(grid.CanPlace(new Vector2Int(2, 2), Vector2Int.one, 0));
        }

        [Test]
        public void SaveData_FurnitureAndHasFlagSurviveCloneAndJson()
        {
            var data = new SaveData { HasFurniture = true, Cash = 10 };
            data.Furniture.Add(new FurnitureRecord { Id = "bed", X = 1, Y = 2, Rotation = 3 });

            SaveData copy = SaveFile.Deserialize(SaveFile.Serialize(data.Clone()));

            Assert.IsTrue(copy.HasFurniture);
            Assert.AreEqual(1, copy.Furniture.Count);
            Assert.AreEqual("bed", copy.Furniture[0].Id);
            Assert.AreEqual(3, copy.Furniture[0].Rotation);
            Assert.AreEqual(10, copy.Cash);
        }

        [Test]
        public void SaveData_NewInstanceHasNoFurnitureSaved()
        {
            Assert.IsFalse(new SaveData().HasFurniture);
        }
    }
}
