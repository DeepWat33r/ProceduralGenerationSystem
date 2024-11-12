using Dungeon.Scripts2D;
using System.Collections;
using UnityEngine;

// Adjust to your actual namespace if different
namespace Hallway
{
    public class HallwayConnect : MonoBehaviour
    {
        private Grid2D<Generator2D.CellType> grid;      // Reference to the dungeon grid
        private Vector2Int gridPosition;                // Position of this hallway in the grid

        public void Initialize(Grid2D<Generator2D.CellType> grid, Vector2Int gridPosition)
        {
            this.grid = grid;
            this.gridPosition = gridPosition;

            // Start the delayed wall removal coroutine
            StartCoroutine(DelayedRemoveWalls());
        }

        // Coroutine to wait a frame before removing walls
        private IEnumerator DelayedRemoveWalls()
        {
            yield return null; // Wait for one frame to ensure all children are recognized
            CheckAndRemoveWalls();
        }

        private void CheckAndRemoveWalls()
        {
            // Check each direction (North, South, East, West)
            CheckAndRemoveWall(Vector2Int.up);    // North
            CheckAndRemoveWall(Vector2Int.down);  // South
            CheckAndRemoveWall(Vector2Int.right); // East
            CheckAndRemoveWall(Vector2Int.left);  // West
        }

        private void CheckAndRemoveWall(Vector2Int direction)
        {
            Vector2Int adjacentPosition = gridPosition + direction;

            // Ensure the adjacent cell is within bounds and is a hallway
            if (grid.InBounds(adjacentPosition) && (grid[adjacentPosition] == Generator2D.CellType.Hallway || grid[adjacentPosition] == Generator2D.CellType.Room))
            {
                // Remove the wall on the current hallway facing the adjacent hallway
                RemoveWall(GetDirectionTag(direction));
            }
        }

        private string GetDirectionTag(Vector2Int direction)
        {
            if (direction == Vector2Int.up) return "Wall_North";
            if (direction == Vector2Int.down) return "Wall_South";
            if (direction == Vector2Int.left) return "Wall_West";
            if (direction == Vector2Int.right) return "Wall_East";
            return "Wall";
        }

        private void RemoveWall(string wallTag)
        {
            // Find and destroy the wall in this hallway that matches the specified direction tag
            Transform wall = FindWallInChildren(transform, wallTag);
            if (wall != null)
            {
                Destroy(wall.gameObject);
                Debug.Log($"Destroyed {wallTag} on hallway at {gridPosition}");
            }
            else
            {
                Debug.LogWarning($"Wall with tag {wallTag} not found on hallway at {gridPosition}");
            }
        }

        private Transform FindWallInChildren(Transform parent, string wallTag)
        {
            // Recursively search for the child with the matching tag
            foreach (Transform child in parent)
            {
                if (child.CompareTag(wallTag))
                    return child;

                // Recursively search within the child's children
                Transform found = FindWallInChildren(child, wallTag);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
