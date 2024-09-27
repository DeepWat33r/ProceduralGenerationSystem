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
        public DecorationType ReservedFor; // New property
        public Cell ReservedBy;            // New property

        public Cell(Vector3 position, CellTag zone, CellSideTag side)
        {
            Position = position;
            Zone = zone;
            Side = side;
            IsOccupied = false;
            ReservedFor = DecorationType.None; // Initialize as None
            ReservedBy = null;                 // Initialize as null
        }
    }
}