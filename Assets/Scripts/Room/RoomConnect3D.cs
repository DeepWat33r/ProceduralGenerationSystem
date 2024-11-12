using System.Collections;
using System.Collections.Generic;
using Dungeon.Scripts3D;
using UnityEngine;

namespace Room
{
    public class RoomConnect3D : MonoBehaviour
    {
        private Grid3D<Generator3D.CellType> grid;

        [SerializeField] private GameObject doorPrefab;  // Door prefab to replace removed walls

        public void Initialize(Grid3D<Generator3D.CellType> grid)
        {
            this.grid = grid;

            // Process wall removal based on adjacent hallways
            StartCoroutine(DelayedRemoveWalls());
        }

        private IEnumerator DelayedRemoveWalls()
        {
            yield return null; // Wait for one frame to ensure all children are recognized
            CheckAndRemoveWalls();
        }

        private void CheckAndRemoveWalls()
        {
            // Check walls on each side: North, South, East, and West (no Up and Down in this case)
            TryRemoveWallsOnSide("Wall_North", Vector3Int.forward);    // North
            TryRemoveWallsOnSide("Wall_South", Vector3Int.back);       // South
            TryRemoveWallsOnSide("Wall_East", Vector3Int.right);       // East
            TryRemoveWallsOnSide("Wall_West", Vector3Int.left);        // West
        }

        private void TryRemoveWallsOnSide(string wallTag, Vector3Int direction)
        {
            // Collect all walls on this side
            List<GameObject> wallsOnSide = new List<GameObject>();
            foreach (Transform child in transform)
            {
                if (child.CompareTag(wallTag))
                {
                    wallsOnSide.Add(child.gameObject);
                }
            }

            List<GameObject> removableWalls = new List<GameObject>();

            foreach (var wall in wallsOnSide)
            {
                Vector3Int wallGridPosition = GetGridPosition(wall.transform.position, direction);
                Vector3Int adjacentPosition = wallGridPosition + direction;

                // Check both the grid boundary and that it’s a hallway at the current Y level
                if (grid.InBounds(adjacentPosition) && grid[adjacentPosition] == Generator3D.CellType.Hallway)
                {
                    removableWalls.Add(wall); // Add wall to the list of removable walls
                }
            }

            if (removableWalls.Count > 0)
            {
                int randomIndex = Random.Range(0, removableWalls.Count);
                GameObject wallToRemove = removableWalls[randomIndex];

                Vector3 wallPosition = wallToRemove.transform.position;
                Quaternion wallRotation = wallToRemove.transform.rotation;

                Destroy(wallToRemove); // Remove the wall

                // Instantiate the door at the wall's position and rotation
                if (doorPrefab != null)
                {
                    Instantiate(doorPrefab, wallPosition, wallRotation, transform);
                }
            }
        }

        private Vector3Int GetGridPosition(Vector3 worldPosition, Vector3Int direction)
        {
            // Adjust the wall position into the room grid cell for precise grid alignment on any floor level
            float offsetAmount = 0.1f;
            Vector3 offsetPosition = worldPosition - new Vector3(direction.x * offsetAmount, 0, direction.z * offsetAmount);

            int x = Mathf.FloorToInt(offsetPosition.x);
            int y = Mathf.FloorToInt(offsetPosition.y);  // Includes Y for height level
            int z = Mathf.FloorToInt(offsetPosition.z);
            return new Vector3Int(x, y, z);
        }
    }
}
