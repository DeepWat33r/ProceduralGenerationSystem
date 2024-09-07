using System.Collections;
using System.Collections.Generic;
using Room;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public Vector2 roomSize = new Vector2(10, 10); // Room size in units
    private Vector2 _previousRoomSize; // To track changes in room size
    public int randomSeed = 123405; // Seed for reproducible randomness
    
    [SerializeField] private Walls walls; // Reference to Walls script
    [SerializeField] private Floor floor;
    [SerializeField] private Ceiling ceiling;

    void Start()
    {
        GetComponents();
        GenerateRoom();
    }

    void Update()
    {
        // Check if room size has changed at runtime
        if (roomSize != _previousRoomSize)
        {
            UpdateRoom();
            _previousRoomSize = roomSize; // Update previous size to the current
        }
    }

    private void GetComponents()
    {
        walls = GetComponent<Walls>();
        if (walls == null) Debug.LogError("Walls component is missing on the GameObject.");
            walls.SetRandomSeed(randomSeed); // Set the seed for the Walls class
        floor = GetComponent<Floor>();
        if (floor == null) Debug.LogError("Floor component is missing on the GameObject.");
            floor.SetRandomSeed(randomSeed); // Set the seed for the Walls class
        ceiling = GetComponent<Ceiling>();
    }

    // Method to generate the room by calling wall generation
    private void GenerateRoom()
    {
        walls.GenerateWalls(roomSize);
        floor.GenerateFloor(roomSize);
        ceiling.GenerateCeiling(roomSize);
    }

    // Method to update the room when the size changes
    private void UpdateRoom()
    {
        GenerateRoom();
    }
}
