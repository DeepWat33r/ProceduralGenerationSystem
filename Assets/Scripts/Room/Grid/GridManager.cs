using System.Collections.Generic;
using UnityEngine;

namespace Room.Grid
{
[RequireComponent(typeof(RoomGenerator))]
public class GridManager : MonoBehaviour
{
    public float cellSize = 1f;                    // Size of each cell in the grid
    private List<Cell> cells = new List<Cell>();   // List to store all cells in the grid
    private RoomGenerator roomGenerator;           // Reference to the RoomGenerator component
    private Vector2 previousRoomSize;              // To track changes in room size

    void Start()
    {
        // Get the RoomGenerator component attached to the same GameObject
        roomGenerator = GetComponent<RoomGenerator>();
        previousRoomSize = roomGenerator.roomSize; // Initialize with the initial room size

        CreateGrid();  // Generate the grid initially
    }

    void Update()
    {
        // Check if room size has changed in the RoomGenerator
        if (roomGenerator.roomSize != previousRoomSize)
        {
            UpdateGrid(); // Update the grid if room size changes
            previousRoomSize = roomGenerator.roomSize; // Update the previous size to the current
        }
    }

    // Method to create the grid within the room
    private void CreateGrid()
    {
        Vector3 basePosition = transform.position; // Base position of the grid, centered in the room
        int gridWidth = Mathf.CeilToInt(roomGenerator.roomSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(roomGenerator.roomSize.y / cellSize);

        cells.Clear(); // Clear existing cells before generating the new grid

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // Calculate the position for each cell, centered in the room
                Vector3 position = basePosition + new Vector3(
                    x * cellSize - roomGenerator.roomSize.x / 2f + cellSize / 2f,
                    0,
                    z * cellSize - roomGenerator.roomSize.y / 2f + cellSize / 2f
                );

                // Determine the zone and side for each cell
                CellTag zone = DetermineZone(position);
                CellSideTag side = DetermineSide(position);

                // Create and store each cell with tags
                cells.Add(new Cell(position, zone, side));
            }
        }

        Debug.Log($"Created grid with {cells.Count} cells.");
    }

    // Method to update the grid when the room size changes
    private void UpdateGrid()
    {
        CreateGrid(); // Simply recreate the grid with the new room size
        Debug.Log("Grid updated due to room size change.");
    }

    // Method to determine the zone of a cell based on its position
    private CellTag DetermineZone(Vector3 position)
    {
        // Define room bounds
        float innerBoundaryX = roomGenerator.roomSize.x / 2f - cellSize; // Margin inside walls
        float innerBoundaryZ = roomGenerator.roomSize.y / 2f - cellSize;

        float outerBoundaryX = roomGenerator.roomSize.x / 2f + cellSize; // Margin outside walls
        float outerBoundaryZ = roomGenerator.roomSize.y / 2f + cellSize;

        // Check if the cell is inside the inner bounds
        bool isInner = position.x > -innerBoundaryX && position.x < innerBoundaryX &&
                       position.z > -innerBoundaryZ && position.z < innerBoundaryZ;

        // Check if the cell is touching or outside the outer bounds
        bool isOuter = position.x <= -outerBoundaryX || position.x >= outerBoundaryX ||
                       position.z <= -outerBoundaryZ || position.z >= outerBoundaryZ;

        // Return the appropriate tag
        return isInner ? CellTag.Inner : CellTag.Outer;
    }

    // Method to determine the side of a cell based on its position
    private CellSideTag DetermineSide(Vector3 position)
    {
        // Determine the cell side based on position relative to room boundaries
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

    // Method to visualize the grid in the Unity editor using Gizmos
    private void OnDrawGizmos()
    {
        if (cells == null || cells.Count == 0) return; // Ensure cells exist before drawing

        foreach (var cell in cells)
        {
            // Visualize the cells based on their zone
            Gizmos.color = cell.zone == CellTag.Inner ? Color.green : Color.red;
            Gizmos.DrawWireCube(cell.position, new Vector3(cellSize, 0.1f, cellSize)); // Draw cells
        }
    }
}
}
