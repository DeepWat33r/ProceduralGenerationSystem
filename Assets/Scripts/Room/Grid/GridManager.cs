using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Room.Grid
{
    [RequireComponent(typeof(RoomGenerator))]
    public class GridManager : MonoBehaviour
    {
        public float cellSize = 1f;
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
            GridWidth = Mathf.CeilToInt(roomGenerator.roomSize.x / cellSize);
            GridHeight = Mathf.CeilToInt(roomGenerator.roomSize.y / cellSize);

            cells.Clear();

            for (int x = 0; x < GridWidth; x++)
            {
                for (int z = 0; z < GridHeight; z++)
                {
                    Vector3 position = basePosition + new Vector3(
                        x * cellSize - roomGenerator.roomSize.x / 2f + cellSize / 2f,
                        0,
                        z * cellSize - roomGenerator.roomSize.y / 2f + cellSize / 2f
                    );

                    CellTag zone = DetermineZone(position);
                    CellSideTag side = DetermineSide(position);

                    cells.Add(new Cell(position, zone, side));
                }
            }

            Debug.Log($"Created grid with {cells.Count} cells.");
        }

        private CellTag DetermineZone(Vector3 position)
        {
            float innerMargin = cellSize;
            float halfWidth = roomGenerator.roomSize.x / 2f;
            float halfHeight = roomGenerator.roomSize.y / 2f;

            bool isInner = position.x > -halfWidth + innerMargin && position.x < halfWidth - innerMargin &&
                           position.z > -halfHeight + innerMargin && position.z < halfHeight - innerMargin;

            return isInner ? CellTag.Inner : CellTag.Outer;
        }

        private CellSideTag DetermineSide(Vector3 position)
        {
            float xRelative = position.x - transform.position.x;
            float zRelative = position.z - transform.position.z;

            if (Mathf.Abs(xRelative) > Mathf.Abs(zRelative))
            {
                return xRelative > 0 ? CellSideTag.East : CellSideTag.West;
            }
            else
            {
                return zRelative > 0 ? CellSideTag.North : CellSideTag.South;
            }
        }

        public int GetCellIndexX(float xPosition)
        {
            float halfWidth = roomGenerator.roomSize.x / 2f;
            return Mathf.FloorToInt((xPosition + halfWidth - cellSize / 2f) / cellSize);
        }

        public int GetCellIndexZ(float zPosition)
        {
            float halfHeight = roomGenerator.roomSize.y / 2f;
            return Mathf.FloorToInt((zPosition + halfHeight - cellSize / 2f) / cellSize);
        }

        public Cell GetCellAt(int xIndex, int zIndex)
        {
            if (xIndex < 0 || xIndex >= GridWidth || zIndex < 0 || zIndex >= GridHeight)
                return null;
            return cells[zIndex * GridWidth + xIndex];
        }

        private void OnDrawGizmos()
        {
            if (cells == null || cells.Count == 0) return;

            foreach (var cell in cells)
            {
                Gizmos.color = cell.IsOccupied ? Color.gray :
                               cell.Zone == CellTag.Inner ? Color.green : Color.red;

                Gizmos.DrawWireCube(cell.Position, new Vector3(cellSize, 0.1f, cellSize));
            }
        }
    }
}
