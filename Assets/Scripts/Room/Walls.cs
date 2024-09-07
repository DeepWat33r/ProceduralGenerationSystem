using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Room
{
[System.Serializable]
    public class WallType
    {
        public GameObject wallPrefab; // The wall prefab
        public float spawnChance;     // The chance of this wall being selected
    }

    public class Walls : MonoBehaviour
    {
        public List<WallType> wallTypes; // List of different wall types with their spawn chances
        public GameObject pillarPrefab;  // Assign pillar prefab here
        private Random _random;    // System.Random instance for seeded randomness
        
        public void SetRandomSeed(int seed) => _random = new Random(seed); // Initialize System.Random with the provided seed

        // Method to generate walls and pillars based on room size
        public void GenerateWalls(Vector2 roomSize)
        {
            // Clear existing walls and pillars if any
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            // Get the Mesh size from the pillar prefab
            MeshFilter pillarMeshFilter = pillarPrefab.GetComponentInChildren<MeshFilter>();
            if (pillarMeshFilter == null) { Debug.LogError("Pillar prefab does not have a MeshFilter component."); return; }

            Vector3 pillarMeshSize = pillarMeshFilter.sharedMesh.bounds.size; // Get the size of the pillar mesh
            Vector3 pillarPrefabScale = pillarPrefab.transform.localScale; // Get the scale of the pillar prefab
            Vector3 scaledPillarMeshSize = Vector3.Scale(pillarMeshSize, pillarPrefabScale); // Adjust pillar size according to the prefab's local scale

            // Calculate the adjusted room size by subtracting the pillar size from the walls' lengths
            float adjustedRoomWidth = roomSize.x - scaledPillarMeshSize.x;
            float adjustedRoomHeight = roomSize.y - scaledPillarMeshSize.z;

            // Use the parent GameObject's position as the base position
            Vector3 basePosition = transform.position;

            // Create walls at specified positions with adjusted lengths
            CreateWall(basePosition + new Vector3(0, 0, roomSize.y / 2 - scaledPillarMeshSize.z / 2), Vector3.right, adjustedRoomWidth, 180f); // North wall
            CreateWall(basePosition + new Vector3(0, 0, -roomSize.y / 2 + scaledPillarMeshSize.z / 2), Vector3.right, adjustedRoomWidth); // South wall
            CreateWall(basePosition + new Vector3(-roomSize.x / 2 + scaledPillarMeshSize.z / 2, 0, 0), Vector3.forward, adjustedRoomHeight, 0f, true); // West wall
            CreateWall(basePosition + new Vector3(roomSize.x / 2 - scaledPillarMeshSize.z / 2, 0, 0), Vector3.forward, adjustedRoomHeight, 180f, true); // East wall

            // Create pillars at the corners of the room
            CreatePillar(basePosition + new Vector3(-roomSize.x / 2 + scaledPillarMeshSize.x / 2, 0, roomSize.y / 2 - scaledPillarMeshSize.z / 2), pillarPrefabScale);
            CreatePillar(basePosition + new Vector3(roomSize.x / 2 - scaledPillarMeshSize.x / 2, 0, roomSize.y / 2 - scaledPillarMeshSize.z / 2), pillarPrefabScale);
            CreatePillar(basePosition + new Vector3(-roomSize.x / 2 + scaledPillarMeshSize.x / 2, 0, -roomSize.y / 2 + scaledPillarMeshSize.z / 2), pillarPrefabScale);
            CreatePillar(basePosition + new Vector3(roomSize.x / 2 - scaledPillarMeshSize.x / 2, 0, -roomSize.y / 2 + scaledPillarMeshSize.z / 2), pillarPrefabScale);
        }

        // Helper method to create individual walls with random modules and optional rotation
        private void CreateWall(Vector3 basePosition, Vector3 direction, float length, float additionalRotation = 0f, bool rotate = false)
        {
            // Get the Mesh size from the default wall prefab for reference
            WallType referenceWallType = GetRandomWallType();
            MeshFilter referenceMeshFilter = referenceWallType.wallPrefab.GetComponentInChildren<MeshFilter>();
            
            Vector3 wallMeshSize = referenceMeshFilter.sharedMesh.bounds.size; // Get the size of the wall mesh
            Vector3 wallPrefabScale = referenceWallType.wallPrefab.transform.localScale; // Get the scale of the wall prefab
            Vector3 scaledWallMeshSize = Vector3.Scale(wallMeshSize, wallPrefabScale); // Adjust mesh size according to the prefab's local scale

            int wallCount = Mathf.Max(1, (int)(length / scaledWallMeshSize.x));
            float scale = (length / wallCount) / scaledWallMeshSize.x; // Calculate scaling factor

            for (int i = 0; i < wallCount; i++)
            {
                // Randomly select a wall type for each module
                WallType selectedWallType = GetRandomWallType();
                if (selectedWallType == null) continue;

                // Calculate the position for each wall module
                Vector3 position = basePosition + direction * (-length / 2 + scaledWallMeshSize.x * scale / 2 + i * scale * scaledWallMeshSize.x);

                // Instantiate the wall prefab at the calculated position with scaling
                GameObject wall = Instantiate(selectedWallType.wallPrefab, position, Quaternion.identity, transform);

                // Rotate the wall based on its orientation and additional rotation if needed
                if (rotate) wall.transform.rotation = Quaternion.Euler(0, 90 + additionalRotation, 0);
                else wall.transform.rotation = Quaternion.Euler(0, additionalRotation, 0);

                // Apply scaling to the wall module
                wall.transform.localScale = new Vector3(scale * wallPrefabScale.x, wallPrefabScale.y, wallPrefabScale.z);

                // Log the position, size, and scale to debug
                Debug.Log($"Wall Position: {position}, Scaled Mesh Size: {scaledWallMeshSize}, Scale: {scale}, Rotated: {rotate}, Additional Rotation: {additionalRotation}");
            }
        }

        // Helper method to create individual pillars
        private void CreatePillar(Vector3 position, Vector3 prefabScale)
        {
            // Instantiate the pillar prefab at the specified position
            GameObject pillar = Instantiate(pillarPrefab, position, Quaternion.identity, transform);

            // Apply scaling to the pillar module
            pillar.transform.localScale = prefabScale;
        }

        // Method to select a wall type based on weighted chances using seeded randomness
        private WallType GetRandomWallType()
        {
            if (wallTypes == null || wallTypes.Count == 0) { Debug.LogError("No wall types available for selection."); return null; }

            float totalChance = 0f;
            foreach (var wallType in wallTypes)
                totalChance += wallType.spawnChance;

            float randomValue = (float)_random.NextDouble() * totalChance; // Use System.Random for seeded randomness
            float cumulativeChance = 0f;

            foreach (var wallType in wallTypes)
            {
                cumulativeChance += wallType.spawnChance;
                if (randomValue < cumulativeChance) return wallType;
            }

            return null; // Fallback, though this should not happen
        }
    }
}
