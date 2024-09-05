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
        CreateWalls();
    }

    void Update()
    {
        // Check if room size has changed at runtime
        if (roomSize != previousRoomSize)
        {
            // Recreate walls based on the updated size
            UpdateWalls();
            previousRoomSize = roomSize; // Update previous size to the current
        }
    }

    void CreateWalls()
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

        int wallCount = Mathf.Max(1, (int)(roomSize.x / scaledMeshSize.x));
        float scale = (roomSize.x / wallCount) / scaledMeshSize.x; // Calculate scaling factor

        for (int i = 0; i < wallCount; i++)
        {
            // Calculate the position for each wall module
            Vector3 position = transform.position + new Vector3(
                -roomSize.x / 2 + scaledMeshSize.x * scale / 2 + i * scale * scaledMeshSize.x, 
                0, 
                -roomSize.y / 2);

            // Instantiate the wall prefab at the calculated position with scaling
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity, transform);

            // Apply scaling to the wall module, combining prefab scale and dynamic scale
            wall.transform.localScale = new Vector3(scale * prefabScale.x, prefabScale.y, prefabScale.z);

            // Log the position, size, and scale to debug
            Debug.Log($"Wall Position: {position}, Scaled Mesh Size: {scaledMeshSize}, Scale: {scale}");
        }
    }

    void UpdateWalls()
    {
        // Recreate walls to reflect new room size
        CreateWalls();
    }
}
