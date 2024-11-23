using UnityEngine;

namespace Dungeon.Scripts2D
{
    public partial class Generator2D
    {
        [Header("Grid Settings")]
        public Color roomColor = Color.green;
        public Color hallwayColor = Color.blue;
        public Color emptyColor = Color.gray;

        public bool showGrid = true;

        private void OnDrawGizmos()
        {
            if (!showGrid || grid == null) return;

            Vector3 cellSize = new Vector3(1, 0.1f, 1); 

            for (int x = 0; x < grid.Size.x; x++)
            {
                for (int y = 0; y < grid.Size.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    
                    Color cellColor = emptyColor;
                    if (grid[pos] == CellType.Room)
                    {
                        cellColor = roomColor;
                    }
                    else if (grid[pos] == CellType.Hallway)
                    {
                        cellColor = hallwayColor;
                    }
                    
                    Vector3 cellPosition = new Vector3(x + 0.5f - grid.Offset.x, 0, y + 0.5f - grid.Offset.y);
                    
                    Gizmos.color = cellColor;
                    Gizmos.DrawWireCube(cellPosition, cellSize);
                    
                    Gizmos.color = new Color(cellColor.r, cellColor.g, cellColor.b, 0.3f);
                    Gizmos.DrawCube(cellPosition, cellSize);
                }
            }
        }
    }
}