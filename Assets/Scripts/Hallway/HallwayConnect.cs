using Dungeon.Scripts2D;
using System.Collections;
using UnityEngine;

namespace Hallway
{
    public class HallwayConnect : MonoBehaviour
    {
        private Grid2D<Generator2D.CellType> grid;      
        private Vector2Int gridPosition;                

        public void Initialize(Grid2D<Generator2D.CellType> grid, Vector2Int gridPosition)
        {
            this.grid = grid;
            this.gridPosition = gridPosition;

            StartCoroutine(DelayedRemoveWalls());
        }

        private IEnumerator DelayedRemoveWalls()
        {
            yield return null; 
            CheckAndRemoveWalls();
        }

        private void CheckAndRemoveWalls()
        {
            CheckAndRemoveWall(Vector2Int.up);    // North
            CheckAndRemoveWall(Vector2Int.down);  // South
            CheckAndRemoveWall(Vector2Int.right); // East
            CheckAndRemoveWall(Vector2Int.left);  // West
        }

        private void CheckAndRemoveWall(Vector2Int direction)
        {
            Vector2Int adjacentPosition = gridPosition + direction;

            if (grid.InBounds(adjacentPosition) && (grid[adjacentPosition] == Generator2D.CellType.Hallway || grid[adjacentPosition] == Generator2D.CellType.Room))
            {
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
            Transform wall = FindWallInChildren(transform, wallTag);
            if (wall != null)
            {
                Destroy(wall.gameObject);
            }
            else
            {
                Debug.LogWarning($"Wall with tag {wallTag} not found on hallway at {gridPosition}");
            }
        }

        private Transform FindWallInChildren(Transform parent, string wallTag)
        {
            foreach (Transform child in parent)
            {
                if (child.CompareTag(wallTag))
                    return child;

                Transform found = FindWallInChildren(child, wallTag);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
