// using Room.Grid;
// using UnityEngine;
//
// namespace Room.ObjectSpawners
// {
//     public class LightSpawner : Spawner
//     {
//         public GameObject lightPrefab; // Assign via Inspector
//
//         public override void Spawn(GridManager gridManager)
//         {
//             Vector2 roomSize = gridManager.RoomSize;
//             Vector3 roomPosition = gridManager.RoomPosition;
//             float wallThickness = gridManager.WallThickness;
//
//             float xMin = roomPosition.x - (roomSize.x / 2f) + wallThickness;
//             float xMax = roomPosition.x + (roomSize.x / 2f) - wallThickness;
//             float zMin = roomPosition.z - (roomSize.y / 2f) + wallThickness;
//             float zMax = roomPosition.z + (roomSize.y / 2f) - wallThickness;
//
//             float yPosition = roomPosition.y + 3f; // Height for lights
//
//             Vector3[] positions = new Vector3[4];
//             positions[0] = new Vector3(xMin, yPosition, zMin); // Bottom-left
//             positions[1] = new Vector3(xMax, yPosition, zMin); // Bottom-right
//             positions[2] = new Vector3(xMax, yPosition, zMax); // Top-right
//             positions[3] = new Vector3(xMin, yPosition, zMax); // Top-left
//
//             for (int i = 0; i < 4; i++)
//             {
//                 Vector3 position = positions[i];
//
//                 Vector3 directionToCenter = (roomPosition - position).normalized;
//                 Quaternion rotation = Quaternion.LookRotation(directionToCenter) * Quaternion.Euler(30f, 0f, 0f);
//
//                 Instantiate(lightPrefab, position, rotation, gridManager.transform);
//
//                 Cell closestCell = gridManager.GetClosestCell(position);
//                 if (closestCell != null)
//                 {
//                     closestCell.IsOccupied = true;
//                 }
//             }
//         }
//     }
// }