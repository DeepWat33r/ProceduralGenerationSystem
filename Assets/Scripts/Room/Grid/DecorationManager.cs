using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Room.Grid;

namespace Room.Grid
{
    [RequireComponent(typeof(GridManager))]
    public class DecorationManager : MonoBehaviour
    {
        public List<DecorationAsset> decorationAssets;
        public int randomSeed = 12345;

        [Range(0f, 1f)]
        public float decorationDensity = 0.3f; // Controls overall decoration density

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

                // Chance to skip this cell entirely
                float placementChance = (float)random.NextDouble();
                if (placementChance > decorationDensity)
                    continue;

                var possibleDecorations = decorationAssets.Where(x => x.zone == cell.Zone).ToList();
                if (possibleDecorations.Count == 0) continue;

                // Random chance to place a decoration based on zone chance
                float zoneChance = GetZoneChance(cell.Zone);
                float rand = (float)random.NextDouble();

                if (rand <= zoneChance)
                {
                    var decoration = PickOneAsset(possibleDecorations);
                    if (decoration != null)
                    {
                        // Random rotation (0, 90, 180, or 270 degrees)
                        int[] possibleRotations = { 0, 90, 180, 270 };
                        int rotationIndex = random.Next(0, possibleRotations.Length);
                        int rotationAngle = possibleRotations[rotationIndex];

                        if (CanPlaceDecoration(cell, decoration, rotationAngle))
                        {
                            PlaceDecoration(cell, decoration, rotationAngle);
                        }
                    }
                }
            }
        }

        private float GetZoneChance(CellTag zone)
        {
            return zone == CellTag.Inner ? 0.5f : 0.2f; // Adjusted values to reduce number of decorations
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

        private bool CanPlaceDecoration(Cell startingCell, DecorationAsset decoration, int rotationAngle)
        {
            int width = (int)decoration.area.x;
            int height = (int)decoration.area.y;

            // Adjust width and height based on rotation
            if (rotationAngle == 90 || rotationAngle == 270)
            {
                int temp = width;
                width = height;
                height = temp;
            }

            var cellsCovered = GetCellsCovered(startingCell, width, height);
            if (cellsCovered == null || cellsCovered.Count == 0) return false;

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

            // Adjust starting indices to center the decoration
            int offsetX = width / 2;
            int offsetZ = height / 2;

            int startIndexX = startX - offsetX + (width % 2 == 0 ? 1 : 0);
            int startIndexZ = startZ - offsetZ + (height % 2 == 0 ? 1 : 0);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int cellX = startIndexX + x;
                    int cellZ = startIndexZ + z;

                    Cell cell = gridManager.GetCellAt(cellX, cellZ);
                    if (cell == null) return null; // Decoration doesn't fit
                    cellsCovered.Add(cell);
                }
            }

            return cellsCovered;
        }

        private void PlaceDecoration(Cell startingCell, DecorationAsset decoration, int rotationAngle)
        {
            int width = (int)decoration.area.x;
            int height = (int)decoration.area.y;

            // Adjust width and height based on rotation
            if (rotationAngle == 90 || rotationAngle == 270)
            {
                int temp = width;
                width = height;
                height = temp;
            }

            var cellsCovered = GetCellsCovered(startingCell, width, height);
            if (cellsCovered == null || cellsCovered.Count == 0) return;

            // Mark cells as occupied
            foreach (var cell in cellsCovered)
            {
                cell.IsOccupied = true;
            }

            // Calculate the center position of the decoration
            Vector3 position = CalculateDecorationPosition(cellsCovered);
            Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);

            Instantiate(decoration.prefab, position, rotation, transform);
        }

        private Vector3 CalculateDecorationPosition(List<Cell> cellsCovered)
        {
            if (cellsCovered == null || cellsCovered.Count == 0)
            {
                Debug.LogError("Cannot calculate decoration position: cellsCovered is null or empty.");
                return Vector3.zero;
            }
            Vector3 sumPosition = Vector3.zero;
            foreach (var cell in cellsCovered)
            {
                sumPosition += cell.Position;
            }
            return sumPosition / cellsCovered.Count;
        }
    }
}
