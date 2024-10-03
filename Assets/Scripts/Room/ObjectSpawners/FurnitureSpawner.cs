// using UnityEngine;
// using Room.Grid;
// using System.Collections.Generic;
// using System.Linq;
//
// namespace Room.ObjectSpawners
// {
//     public class FurnitureSpawner : Spawner
//     {
//         public List<DecorationAsset> furnitureAssets; // Assign via Inspector
//         public float furnitureDensity = 0.05f;        // Adjust as needed
//
//         private System.Random random;
//
//         public override void Spawn(GridManager gridManager)
//         {
//             random = new System.Random();
//
//             // Collect available inner cells
//             List<Cell> availableCells = gridManager.Cells
//                 .Where(cell => !cell.IsOccupied && cell.CellTag == CellTag.Inner)
//                 .ToList();
//
//             // Shuffle the cells to randomize placement
//             Shuffle(availableCells);
//
//             // Calculate the desired number of tables based on inner cell count
//             int desiredTableCount = Mathf.Max(1, Mathf.RoundToInt(gridManager.InnerCellCount * furnitureDensity));
//
//             int tablesPlaced = 0;
//
//             foreach (var cell in availableCells)
//             {
//                 if (tablesPlaced >= desiredTableCount)
//                     break;
//
//                 if (cell.IsOccupied)
//                     continue;
//
//                 DecorationAsset tableAsset = GetRandomTableAsset();
//                 if (tableAsset == null)
//                     continue;
//
//                 // Try different rotations if rotation is allowed
//                 List<int> rotations = tableAsset.canRotate ? new List<int> { 0, 90, 180, 270 } : new List<int> { 0 };
//                 Shuffle(rotations);
//
//                 bool placed = false;
//                 foreach (var rotation in rotations)
//                 {
//                     var cellsToOccupy = GetCellsForFurniture(cell, tableAsset, gridManager, rotation+90);
//                     if (cellsToOccupy == null)
//                         continue;
//
//                     if (CanPlaceFurniture(cellsToOccupy))
//                     {
//                         PlaceFurniture(cellsToOccupy, tableAsset, gridManager, rotation);
//
//                         // Mark occupied cells after rotation
//                         MarkOccupiedCells(cellsToOccupy);
//
//                         placed = true;
//                         tablesPlaced++;
//                         break;
//                     }
//                 }
//             }
//         }
//
//         private DecorationAsset GetRandomTableAsset()
//         {
//             if (furnitureAssets == null || furnitureAssets.Count == 0)
//                 return null;
//
//             // Only select assets with DecorationType.Table
//             var tableAssets = furnitureAssets.Where(asset => asset.decorationType == DecorationType.Table).ToList();
//             if (tableAssets.Count == 0)
//                 return null;
//
//             // Simple random selection
//             int index = random.Next(tableAssets.Count);
//             return tableAssets[index];
//         }
//
//         private bool CanPlaceFurniture(List<Cell> cellsToOccupy)
//         {
//             foreach (var cell in cellsToOccupy)
//             {
//                 if (cell.IsOccupied || cell.CellTag != CellTag.Inner)
//                     return false;
//             }
//             return true;
//         }
//
//         private void PlaceFurniture(List<Cell> cellsToOccupy, DecorationAsset furniture, GridManager gridManager, int rotation)
//         {
//             // Calculate the center position of the furniture based on its size and rotation
//             Vector3 position = CalculateFurniturePosition(cellsToOccupy);
//
//             Quaternion rot = Quaternion.Euler(0f, rotation, 0f);
//
//             // Instantiate the furniture
//             Instantiate(furniture.prefab, position, rot, gridManager.transform);
//         }
//
//
//         private Vector3 CalculateFurniturePosition(List<Cell> cellsCovered)
//         {
//             Vector3 sumPosition = Vector3.zero;
//             foreach (var cell in cellsCovered)
//             {
//                 sumPosition += cell.Position;
//             }
//             return sumPosition / cellsCovered.Count;
//         }
//
//         private void MarkOccupiedCells(List<Cell> cells)
//         {
//             foreach (var cell in cells)
//             {
//                 cell.IsOccupied = true;
//             }
//         }
//
//         private List<Cell> GetCellsForFurniture(Cell startingCell, DecorationAsset furniture, GridManager gridManager, int rotation)
//         {
//             List<Cell> cellsToOccupy = new List<Cell>();
//
//             int width = furniture.size.x;
//             int height = furniture.size.y;
//
//             int normalizedRotation = rotation % 360;
//
//             // Adjust width and height based on rotation
//             if (normalizedRotation == 90 || normalizedRotation == 270)
//             {
//                 int temp = width;
//                 width = height;
//                 height = temp;
//             }
//
//             int startX = gridManager.GetCellIndexX(startingCell.Position.x);
//             int startZ = gridManager.GetCellIndexZ(startingCell.Position.z);
//
//             // Adjust starting indices to center the furniture
//             int offsetX = (width - 1) / 2;
//             int offsetZ = (height - 1) / 2;
//
//             int startIndexX = startX - offsetX;
//             int startIndexZ = startZ - offsetZ;
//
//             for (int x = 0; x < width; x++)
//             {
//                 for (int z = 0; z < height; z++)
//                 {
//                     int cellX = startIndexX + x;
//                     int cellZ = startIndexZ + z;
//
//                     Cell cell = gridManager.GetCellAt(cellX, cellZ);
//                     if (cell == null)
//                         return null; // Furniture doesn't fit in the grid
//
//                     cellsToOccupy.Add(cell);
//                 }
//             }
//
//             return cellsToOccupy;
//         }
//
//         private void Shuffle<T>(List<T> list)
//         {
//             int n = list.Count;
//             while (n > 1)
//             {
//                 n--;
//                 int k = random.Next(n + 1);
//                 T value = list[k];
//                 list[k] = list[n];
//                 list[n] = value;
//             }
//         }
//     }
// }
