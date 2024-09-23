using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Room.Grid
{
    [RequireComponent(typeof(GridManager))]
    public class DecorationManager : MonoBehaviour
    {
        public List<DecorationAsset> decorationAssets;
        public int randomSeed = 12345;
        private GridManager gridManager;
        private System.Random random;

        void Awake()
        {
            gridManager = GetComponent<GridManager>();
        }

        public void SetRandomSeed(int seed)
        {
            randomSeed = seed;
            random = new System.Random(randomSeed);
        }

        public void PopulateGridWithDecorations()
        {
            if (random == null) random = new System.Random(randomSeed);

            foreach (var cell in gridManager.Cells)
            {
                if (cell.IsOccupied) continue;

                var possibleDecorations = decorationAssets.Where(x => x.zone == cell.Zone).ToList();
                if (possibleDecorations.Count == 0) continue;

                float zoneChance = GetZoneChance(cell.Zone);
                float rand = (float)random.NextDouble();

                if (rand <= zoneChance)
                {
                    var decoration = PickOneAsset(possibleDecorations);
                    if (decoration != null)
                    {
                        if (CanPlaceDecoration(cell, decoration))
                        {
                            PlaceDecoration(cell, decoration);
                        }
                    }
                }
            }
        }

        private float GetZoneChance(CellTag zone)
        {
            return zone == CellTag.Inner ? 0.7f : 0.3f;
        }

        private DecorationAsset PickOneAsset(List<DecorationAsset> possibleDecorations)
        {
            float totalChance = possibleDecorations.Sum(x => x.chances);
            float randomValue = (float)random.NextDouble() * totalChance;
            float cumulativeChance = 0f;

            foreach (var decoration in possibleDecorations)
            {
                cumulativeChance += decoration.chances;
                if (randomValue <= cumulativeChance)
                {
                    return decoration;
                }
            }
            return null;
        }

        private bool CanPlaceDecoration(Cell startingCell, DecorationAsset decoration)
        {
            int width = (int)decoration.area.x;
            int height = (int)decoration.area.y;

            var cellsCovered = GetCellsCovered(startingCell, width, height);
            if (cellsCovered == null) return false;

            foreach (var cell in cellsCovered)
            {
                if (cell.IsOccupied) return false;
            }

            return true;
        }

        private List<Cell> GetCellsCovered(Cell startingCell, int width, int height)
        {
            List<Cell> cellsCovered = new List<Cell>();

            int startX = gridManager.GetCellIndexX(startingCell.Position.x);
            int startZ = gridManager.GetCellIndexZ(startingCell.Position.z);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int cellX = startX + x;
                    int cellZ = startZ + z;

                    Cell cell = gridManager.GetCellAt(cellX, cellZ);
                    if (cell == null) return null;
                    cellsCovered.Add(cell);
                }
            }

            return cellsCovered;
        }

        private void PlaceDecoration(Cell startingCell, DecorationAsset decoration)
        {
            var cellsCovered = GetCellsCovered(startingCell, (int)decoration.area.x, (int)decoration.area.y);
            foreach (var cell in cellsCovered)
            {
                cell.IsOccupied = true;
            }

            Vector3 position = startingCell.Position;
            Quaternion rotation = GetRotation(startingCell.Side);

            Instantiate(decoration.prefab, position, rotation, transform);
        }

        private Quaternion GetRotation(CellSideTag side)
        {
            switch (side)
            {
                case CellSideTag.North: return Quaternion.Euler(0, 0, 0);
                case CellSideTag.South: return Quaternion.Euler(0, 180, 0);
                case CellSideTag.East: return Quaternion.Euler(0, 90, 0);
                case CellSideTag.West: return Quaternion.Euler(0, -90, 0);
                default: return Quaternion.identity;
            }
        }
    }
}
