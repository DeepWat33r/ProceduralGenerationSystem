using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{ 
    public GameObject wallPrefab; // Assign your wall prefab here
    public Vector2 roomSize = new Vector2(10, 10); // Room size in units
    private Vector2 previousRoomSize; // To track changes in room size

    void Start()
    {
        CreateRoom();
    }

    void Update()
    {
        // Check if room size has changed at runtime
        if (roomSize != previousRoomSize)
        {
            // Recreate walls based on the updated size
            UpdateRoom();
            previousRoomSize = roomSize; // Update previous size to the current
        }
    }

    void CreateRoom()
    {
        // Clear existing walls if any
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Get the Mesh size from the prefab to use for positioning
        MeshFilter meshFilter = wallPrefab.GetComponentInChildren<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("Wall prefab does not have a MeshFilter component.");
            return;
        }

        Vector3 meshSize = meshFilter.sharedMesh.bounds.size; // Get the size of the mesh
        Vector3 prefabScale = wallPrefab.transform.localScale; // Get the scale of the prefab

        // Adjust mesh size according to the prefab's local scale
        Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale);

        // Create front wall (aligned along +z axis)
        CreateWall(new Vector3(0, 0, roomSize.y / 2 - scaledMeshSize.z / 2), Vector3.right, scaledMeshSize, prefabScale, roomSize.x);

        // Create back wall (aligned along -z axis)
        CreateWall(new Vector3(0, 0, -roomSize.y / 2 + scaledMeshSize.z / 2), Vector3.right, scaledMeshSize, prefabScale, roomSize.x);

        // Create left wall (aligned along -x axis, rotated 90 degrees)
        CreateWall(new Vector3(-roomSize.x / 2 + scaledMeshSize.z / 2, 0, 0), Vector3.forward, scaledMeshSize, prefabScale, roomSize.y, true);

        // Create right wall (aligned along +x axis, rotated 90 degrees)
        CreateWall(new Vector3(roomSize.x / 2 - scaledMeshSize.z / 2, 0, 0), Vector3.forward, scaledMeshSize, prefabScale, roomSize.y, true);
    }

    void CreateWall(Vector3 basePosition, Vector3 direction, Vector3 scaledMeshSize, Vector3 prefabScale, float length, bool rotate = false)
    {
        int wallCount = Mathf.Max(1, (int)(length / scaledMeshSize.x));
        float scale = (length / wallCount) / scaledMeshSize.x; // Calculate scaling factor

        for (int i = 0; i < wallCount; i++)
        {
            // Calculate the position for each wall module
            Vector3 position = basePosition + direction * (-length / 2 + scaledMeshSize.x * scale / 2 + i * scale * scaledMeshSize.x);

            // Instantiate the wall prefab at the calculated position with scaling
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);

            // Rotate the wall if needed (for left and right walls)
            if (rotate)
            {
                wall.transform.rotation = Quaternion.Euler(0, 90, 0);
            }

            // Apply scaling to the wall module
            wall.transform.localScale = new Vector3(scale * prefabScale.x, prefabScale.y, prefabScale.z);

            // Log the position, size, and scale to debug
            Debug.Log($"Wall Position: {position}, Scaled Mesh Size: {scaledMeshSize}, Scale: {scale}, Rotated: {rotate}");
        }
    }

    void UpdateRoom()
    {
        // Recreate the room to reflect new room size
        CreateRoom();
    }
}
