using UnityEngine;
using System.Collections.Generic;

namespace Dungeon.Scripts3D
{
    public partial class Generator3D
    {
        [Header("Grid Settings")]
        // Gizmo colors for different cell types
        [SerializeField] private Color roomColor = Color.green;
        [SerializeField] private Color hallwayColor = Color.blue;
        [SerializeField] private Color stairsColor = Color.yellow;
        [SerializeField] private Color emptyColor = Color.clear;

        // Enable/disable grid visualization
        public bool showGrid = true;
        private void OnDrawGizmos()
        {
            if (!showGrid || grid == null) return;

            foreach (var cell in GetGridCells())
            {
                Vector3 center = new Vector3(cell.Key.x + 0.5f, cell.Key.y + 0.5f, cell.Key.z + 0.5f);
                DrawCellGizmo(center, cell.Value);
            }
        }

        // Helper method to get all cells in the grid
        private Dictionary<Vector3Int, CellType> GetGridCells()
        {
            Dictionary<Vector3Int, CellType> cells = new Dictionary<Vector3Int, CellType>();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int z = 0; z < size.z; z++)
                    {
                        Vector3Int position = new Vector3Int(x, y, z);
                        if (grid.InBounds(position))
                        {
                            cells[position] = grid[position];
                        }
                    }
                }
            }
            return cells;
        }

        // Helper method to draw Gizmo for each cell based on its type
        private void DrawCellGizmo(Vector3 position, CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Room:
                    Gizmos.color = roomColor;
                    break;
                case CellType.Hallway:
                    Gizmos.color = hallwayColor;
                    break;
                case CellType.Stairs:
                    Gizmos.color = stairsColor;
                    break;
                default:
                    Gizmos.color = emptyColor;
                    break;
            }

            Gizmos.DrawCube(position, Vector3.one);
        }
    }
}
