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
        public float decorationDensity = 0.3f;

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
                if (cell.IsOccupied || cell.ReservedFor != DecorationType.None) continue;

                // Chance to skip this cell entirely
                float placementChance = (float)random.NextDouble();
                if (placementChance > decorationDensity)
                    continue;

                // Exclude chairs from initial placement
                var possibleDecorations = decorationAssets
                    .Where(x => x.zone == cell.Zone && x.decorationType != DecorationType.None && x.decorationType != DecorationType.Chair)
                    .ToList();
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

            // Place associated decorations after initial placement
            PlaceReservedDecorations();
        }

        private float GetZoneChance(CellTag zone)
        {
            return zone == CellTag.Inner ? 0.5f : 0.2f;
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
                if (cell.IsOccupied || cell.ReservedFor != DecorationType.None)
                    return false;

                // For tables, ensure there is space for chairs
                if (decoration.decorationType == DecorationType.Table)
                {
                    List<Cell> adjacentCells = GetAdjacentCells(cell);
                    if (adjacentCells.Count == 0)
                        return false; // No adjacent cells available
                }
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
                    if (cell == null) return null;
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

            // Reserve adjacent cells if the decoration has associated decorations
            if (decoration.hasAssociatedDecorations)
            {
                ReserveAdjacentCells(cellsCovered, decoration.associatedDecorationType);
            }

            // Calculate the center position of the decoration
            Vector3 position = CalculateDecorationPosition(cellsCovered);
            Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);

            Instantiate(decoration.prefab, position, rotation, transform);
        }

        private void ReserveAdjacentCells(List<Cell> cellsCovered, DecorationType reservationType)
        {
            int reservationsMade = 0;
            foreach (var cell in cellsCovered)
            {
                // Get adjacent cells
                List<Cell> adjacentCells = GetAdjacentCells(cell);
                //Debug.Log($"Table at ({cell.Position.x}, {cell.Position.z}) has {adjacentCells.Count} adjacent cells.");
                foreach (var adjacentCell in adjacentCells)
                {
                    if (!adjacentCell.IsOccupied && adjacentCell.ReservedFor == DecorationType.None)
                    {
                        adjacentCell.ReservedFor = reservationType;
                        adjacentCell.ReservedBy = cell; // Keep track of the table cell that reserved it
                        reservationsMade++;
                        //Debug.Log($"Reserved cell at ({adjacentCell.Position.x}, {adjacentCell.Position.z}) for {reservationType}.");
                    }
                    else
                    {
                        //Debug.Log($"Cannot reserve cell at ({adjacentCell.Position.x}, {adjacentCell.Position.z}) - Occupied: {adjacentCell.IsOccupied}, ReservedFor: {adjacentCell.ReservedFor}");
                    }
                }
            }
            //Debug.Log($"Reserved {reservationsMade} cells for {reservationType}.");
        }

        private List<Cell> GetAdjacentCells(Cell cell)
        {
            List<Cell> adjacentCells = new List<Cell>();

            int cellX = gridManager.GetCellIndexX(cell.Position.x);
            int cellZ = gridManager.GetCellIndexZ(cell.Position.z);

            int[] dx = { -1, 0, 1, 0 };
            int[] dz = { 0, 1, 0, -1 };

            for (int i = 0; i < 4; i++)
            {
                int adjacentX = cellX + dx[i];
                int adjacentZ = cellZ + dz[i];

                Cell adjacentCell = gridManager.GetCellAt(adjacentX, adjacentZ);
                if (adjacentCell != null)
                {
                    adjacentCells.Add(adjacentCell);
                }
                else
                {
                    //Debug.Log($"Adjacent cell at index ({adjacentX}, {adjacentZ}) is out of bounds.");
                }
            }

            return adjacentCells;
        }

        public void PlaceReservedDecorations()
        {
            int chairPlacementAttempts = 0;
            foreach (var cell in gridManager.Cells)
            {
                if (!cell.IsOccupied && cell.ReservedFor == DecorationType.Chair)
                {
                    // Get chair decorations
                    var possibleDecorations = decorationAssets
                        .Where(x => x.decorationType == DecorationType.Chair && !x.hasAssociatedDecorations)
                        .ToList();

                    if (possibleDecorations.Count == 0)
                    {
                        //Debug.LogWarning("No chair decorations found to place.");
                        continue;
                    }

                    var decoration = possibleDecorations[0]; // Or implement selection logic
                    if (decoration != null)
                    {
                        // Calculate rotation angle based on position relative to the table
                        int rotationAngle = CalculateChairRotation(cell, cell.ReservedBy);
                        PlaceDecorationAtCell(cell, decoration, rotationAngle);
                        chairPlacementAttempts++;
                    }

                    // Clear the reservation
                    cell.ReservedFor = DecorationType.None;
                    cell.ReservedBy = null;
                }
            }
            //Debug.Log($"Attempted to place chairs at {chairPlacementAttempts} reserved cells.");
        }


        private int CalculateChairRotation(Cell chairCell, Cell tableCell)
        {
            Vector3 direction = (chairCell.Position - tableCell.Position).normalized;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            int rotationAngle = Mathf.RoundToInt(angle / 90f) * 90;
            return rotationAngle;
        }

        private void PlaceDecorationAtCell(Cell cell, DecorationAsset decoration, int rotationAngle)
        {
            // Check if the decoration can be placed
            if (CanPlaceDecoration(cell, decoration, rotationAngle))
            {
                // Mark cell as occupied
                cell.IsOccupied = true;

                // Place the decoration
                Vector3 position = cell.Position;
                Quaternion rotation = Quaternion.Euler(0, rotationAngle, 0);
                Instantiate(decoration.prefab, position, rotation, transform);
            }
        }

        private Vector3 CalculateDecorationPosition(List<Cell> cellsCovered)
        {
            if (cellsCovered == null || cellsCovered.Count == 0)
            {
                //Debug.LogError("Cannot calculate decoration position: cellsCovered is null or empty.");
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
