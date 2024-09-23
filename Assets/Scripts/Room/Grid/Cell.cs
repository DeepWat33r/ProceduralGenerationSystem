using UnityEngine;

namespace Room.Grid
{
    public enum CellTag { Inner, Outer }
    public enum CellSideTag { North, South, East, West, Center }

    public class Cell
    {
        public Vector3 Position;
        public CellTag Zone;
        public CellSideTag Side;
        public bool IsOccupied;

        public Cell(Vector3 position, CellTag zone, CellSideTag side)
        {
            Position = position;
            Zone = zone;
            Side = side;
            IsOccupied = false;
        }

        public override string ToString()
        {
            return $"{Position} + {Side}";
        }
    }
}