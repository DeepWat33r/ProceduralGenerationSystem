using System.Collections.Generic;
using UnityEngine;

namespace Room.Grid
{
    [RequireComponent(typeof(RoomGenerator))]
    public class GridManager : MonoBehaviour
    {
        public float cellSize = 1f;
        public float wallThickness = 0.5f; 
        private List<Cell> cells = new List<Cell>();
        private RoomGenerator roomGenerator;

        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }
        public List<Cell> Cells => cells;

        void Awake()
        {
            roomGenerator = GetComponent<RoomGenerator>();
        }

        public void CreateGrid()
        {
            Vector3 basePosition = transform.position;

            float effectiveRoomWidth = roomGenerator.roomSize.x - 2 * wallThickness;
            float effectiveRoomHeight = roomGenerator.roomSize.y - 2 * wallThickness;

            GridWidth = Mathf.RoundToInt(effectiveRoomWidth / cellSize);
            GridHeight = Mathf.RoundToInt(effectiveRoomHeight / cellSize);

            effectiveRoomWidth = GridWidth * cellSize;
            effectiveRoomHeight = GridHeight * cellSize;

            float startX = basePosition.x - effectiveRoomWidth / 2f + cellSize / 2f;
            float startZ = basePosition.z - effectiveRoomHeight / 2f + cellSize / 2f;
    
            float gridYPosition = basePosition.y;  

            cells.Clear();

            for (int z = 0; z < GridHeight; z++)
            {
                for (int x = 0; x < GridWidth; x++)
                {
                    Vector3 position = new Vector3(
                        startX + x * cellSize,
                        gridYPosition, 
                        startZ + z * cellSize
                    );

                    CellTag zone = DetermineZone(x, z);
                    CellSideTag side = DetermineSide(x, z);

                    cells.Add(new Cell(position, zone, side));
                }
            }

            //Debug.Log($"Created grid with {cells.Count} cells.");
        }


        private CellTag DetermineZone(int xIndex, int zIndex)
        {
            // Cells at the edges are considered outer cells
            if (xIndex == 0 || xIndex == GridWidth - 1 || zIndex == 0 || zIndex == GridHeight - 1)
            {
                return CellTag.Outer;
            }
            else
            {
                return CellTag.Inner;
            }
        }

        private CellSideTag DetermineSide(int xIndex, int zIndex)
        {
            if (zIndex == GridHeight - 1)
                return CellSideTag.North;
            else if (zIndex == 0)
                return CellSideTag.South;
            else if (xIndex == GridWidth - 1)
                return CellSideTag.East;
            else if (xIndex == 0)
                return CellSideTag.West;
            else
                return CellSideTag.Center;
        }

        public int GetCellIndexX(float xPosition)
        {
            float halfWidth = (roomGenerator.roomSize.x - 2 * wallThickness) / 2f;
            float startX = transform.position.x - halfWidth;

            int xIndex = Mathf.RoundToInt((xPosition - startX) / cellSize - 0.5f);
            return xIndex;
        }

        public int GetCellIndexZ(float zPosition)
        {
            float halfHeight = (roomGenerator.roomSize.y - 2 * wallThickness) / 2f;
            float startZ = transform.position.z - halfHeight;

            int zIndex = Mathf.RoundToInt((zPosition - startZ) / cellSize - 0.5f);
            return zIndex;
        }

        public Cell GetCellAt(int xIndex, int zIndex)
        {
            if (xIndex < 0 || xIndex >= GridWidth || zIndex < 0 || zIndex >= GridHeight)
                return null;
            return cells[zIndex * GridWidth + xIndex];
        }

        // private void OnDrawGizmos()
        // {
        //     if (cells == null || cells.Count == 0) return;
        //
        //     foreach (var cell in cells)
        //     {
        //         if (cell.IsOccupied)
        //         {
        //             Gizmos.color = Color.red; // Occupied cells are red
        //         }
        //         else if (cell.ReservedFor != DecorationType.None)
        //         {
        //             Gizmos.color = Color.blue; // Reserved cells are blue
        //         }
        //         else
        //         {
        //             Gizmos.color = cell.Zone == CellTag.Inner ? Color.green : Color.yellow; // Inner unoccupied cells are green, outer are yellow
        //         }
        //
        //         Gizmos.DrawWireCube(cell.Position, new Vector3(cellSize, 0.1f, cellSize));
        //     }
        // }
    }
}
