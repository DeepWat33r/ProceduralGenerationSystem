using System.Collections;
using System.Collections.Generic;
using Dungeon.Scripts2D;
using UnityEngine;

namespace Room
{
    public class RoomConnect : MonoBehaviour
    {
        private Grid2D<Generator2D.CellType> grid;
    
        [SerializeField] private GameObject doorPrefab;  // Door prefab to replace removed walls

        public void Initialize(Grid2D<Generator2D.CellType> grid)
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
            // Check walls on each side: North, South, East, West
            TryRemoveWallsOnSide("Wall_North", Vector2Int.up);    // North
            TryRemoveWallsOnSide("Wall_South", Vector2Int.down);  // South
            TryRemoveWallsOnSide("Wall_East", Vector2Int.right);  // East
            TryRemoveWallsOnSide("Wall_West", Vector2Int.left);   // West
        }

        private void TryRemoveWallsOnSide(string wallTag, Vector2Int direction)
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
                Vector2Int wallGridPosition = GetGridPosition(wall.transform.position, direction);
                Vector2Int adjacentPosition = wallGridPosition + direction;

                if (grid.InBounds(adjacentPosition) && grid[adjacentPosition] == Generator2D.CellType.Hallway)
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
                    //Debug.Log($"Placed a door at {wallPosition} facing {wallTag}");
                }
            }
        }

        private Vector2Int GetGridPosition(Vector3 worldPosition, Vector2Int direction)
        {
            float offsetAmount = 0.1f;
            Vector3 offsetPosition = worldPosition - new Vector3(direction.x * offsetAmount, 0, direction.y * offsetAmount);
        
            int x = Mathf.FloorToInt(offsetPosition.x);
            int y = Mathf.FloorToInt(offsetPosition.z);
            return new Vector2Int(x, y);
        }
    }
}