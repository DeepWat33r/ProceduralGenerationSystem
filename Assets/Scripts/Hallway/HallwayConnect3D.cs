using Dungeon.Scripts3D;
using System.Collections;
using UnityEngine;

namespace Hallway
{
    public class HallwayConnect3D : MonoBehaviour
    {
        private Grid3D<Generator3D.CellType> grid;   // Reference to the 3D dungeon grid
        private Vector3Int gridPosition;             // Position of this hallway in the 3D grid

        public void Initialize(Grid3D<Generator3D.CellType> grid, Vector3Int gridPosition)
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
            // Check each direction in 3D space (North, South, East, West, Up, Down)
            CheckAndRemoveWall(Vector3Int.forward);   // North
            CheckAndRemoveWall(Vector3Int.back);      // South
            CheckAndRemoveWall(Vector3Int.right);     // East
            CheckAndRemoveWall(Vector3Int.left);      // West
        }

        private void CheckAndRemoveWall(Vector3Int direction)
        {
            Vector3Int adjacentPosition = gridPosition + direction;

            // Ensure the adjacent cell is within bounds and is either a hallway or a room
            if (grid.InBounds(adjacentPosition) && 
                (grid[adjacentPosition] == Generator3D.CellType.Hallway || grid[adjacentPosition] == Generator3D.CellType.Room || grid[adjacentPosition] == Generator3D.CellType.Stairs))
            {
                // Remove the wall on the current hallway facing the adjacent hallway
                RemoveWall(GetDirectionTag(direction));
            }
        }

        private string GetDirectionTag(Vector3Int direction)
        {
            if (direction == Vector3Int.forward) return "Wall_North";
            if (direction == Vector3Int.back) return "Wall_South";
            if (direction == Vector3Int.left) return "Wall_West";
            if (direction == Vector3Int.right) return "Wall_East";
            return "Wall";
        }

        private void RemoveWall(string wallTag)
        {
            // Find and destroy the wall in this hallway that matches the specified direction tag
            Transform wall = FindWallInChildren(transform, wallTag);
            if (wall != null)
            {
                Destroy(wall.gameObject);
                // Debug.Log($"Destroyed {wallTag} on hallway at {gridPosition}");
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
