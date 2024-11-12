using UnityEngine;

namespace Dungeon.Scripts2D
{
    public partial class Generator2D
    {
        [Header("Grid Settings")]
        // Colors for different cell types in the grid visualization
        public Color roomColor = Color.green;
        public Color hallwayColor = Color.blue;
        public Color emptyColor = Color.gray;

        // Enable/disable grid visualization
        public bool showGrid = true;

        private void OnDrawGizmos()
        {
            if (!showGrid || grid == null) return;

            Vector3 cellSize = new Vector3(1, 0.1f, 1); // Adjust cell height for visualization clarity

            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // Determine color based on cell type
                    Color cellColor = emptyColor;
                    if (grid[pos] == CellType.Room)
                    {
                        cellColor = roomColor;
                    }
                    else if (grid[pos] == CellType.Hallway)
                    {
                        cellColor = hallwayColor;
                    }

                    // Calculate the position of the cell center
                    Vector3 cellPosition = new Vector3(x + 0.5f - grid.Offset.x, 0, y + 0.5f - grid.Offset.y);

                    // Draw a wireframe cube for each cell in the grid
                    Gizmos.color = cellColor;
                    Gizmos.DrawWireCube(cellPosition, cellSize);

                    // Fill in the grid cell if needed
                    Gizmos.color = new Color(cellColor.r, cellColor.g, cellColor.b, 0.3f);
                    Gizmos.DrawCube(cellPosition, cellSize);
                }
            }
        }
    }
}