using Dungeon.Scripts3D;
using System.Collections;
using UnityEngine;

namespace Hallway
{
    public class HallwayConnect3D : MonoBehaviour
    {
        private Grid3D<Generator3D.CellType> grid;  
        private Vector3Int gridPosition;             

        public void Initialize(Grid3D<Generator3D.CellType> grid, Vector3Int gridPosition)
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
            CheckAndRemoveWall(Vector3Int.forward);   // North
            CheckAndRemoveWall(Vector3Int.back);      // South
            CheckAndRemoveWall(Vector3Int.right);     // East
            CheckAndRemoveWall(Vector3Int.left);      // West
        }

        private void CheckAndRemoveWall(Vector3Int direction)
        {
            Vector3Int adjacentPosition = gridPosition + direction;

            if (grid.InBounds(adjacentPosition) && 
                (grid[adjacentPosition] == Generator3D.CellType.Hallway || grid[adjacentPosition] == Generator3D.CellType.Room || grid[adjacentPosition] == Generator3D.CellType.Stairs))
            {
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
