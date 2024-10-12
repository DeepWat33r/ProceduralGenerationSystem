using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Room
{
    [System.Serializable]
    public class FloorPrefab
    {
        public GameObject floorPrefab; // The floor prefab
        public float spawnChance;      // The chance of this floor prefab being selected
    }

    [System.Serializable]
    public class FloorType
    {
        public string typeName;                 // Name of the floor type
        public List<FloorPrefab> floorPrefabs;  // List of floor prefabs for this type
    }
    public class Floor : MonoBehaviour
    {
        public List<FloorType> floorTypes; // List of the four different floor types
        private Random _random;      // System.Random instance for seeded randomness

        // Method to set the random seed
        public void SetRandomSeed(int seed) => _random = new Random(seed); // Initialize System.Random with the provided seed


        // Method to generate the floor based on room size
        public void GenerateFloor(Vector2 roomSize)
        {
            // Use the parent GameObject's position as the base position
            Vector3 basePosition = transform.position;

            // Calculate number of tiles needed based on room size and tile size (assumed 2x2)
            int tileCountX = Mathf.CeilToInt(roomSize.x / 0.5f);
            int tileCountZ = Mathf.CeilToInt(roomSize.y / 0.5f);

            // Randomly select one of the four floor types
            FloorType selectedFloorType = GetRandomFloorType();
            if (selectedFloorType == null) { Debug.LogError("Failed to select a floor type."); return; }

            // Calculate tile size for scaling
            Vector2 tileSize = new Vector2(roomSize.x / tileCountX, roomSize.y / tileCountZ);

            // Generate the floor tiles
            for (int x = 0; x < tileCountX; x++)
            {
                for (int z = 0; z < tileCountZ; z++)
                {
                    // Calculate the position for each floor tile, centering in the room
                    Vector3 position = basePosition + new Vector3(x * tileSize.x - roomSize.x / 2f + tileSize.x / 2f, 0, z * tileSize.y - roomSize.y / 2f + tileSize.y / 2f);

                    // Select a random prefab from the chosen floor type
                    FloorPrefab selectedFloorPrefab = GetRandomFloorPrefab(selectedFloorType);
                    if (selectedFloorPrefab == null) continue;

                    // Instantiate the floor tile
                    GameObject floorTile = Instantiate(selectedFloorPrefab.floorPrefab, position, Quaternion.identity, transform);

                    // Align the floor tile by its lowest point
                    AlignFloorTile(floorTile);

                    // Scale the floor tile to fit the room perfectly
                    ScaleFloorTile(floorTile, tileSize);

                    // Log the position and type for debugging
                    //Debug.Log($"Floor Tile Position: {position}, Type: {selectedFloorType.typeName}");
                }
            }
        }

        // Helper method to select a random floor type
        private FloorType GetRandomFloorType()
        {
            if (floorTypes == null || floorTypes.Count == 0) { Debug.LogError("No floor types available for selection."); return null; }

            // Since each type has equal probability, simply use Random to select one
            int selectedIndex = _random.Next(floorTypes.Count);
            return floorTypes[selectedIndex];
        }

        // Helper method to select a random floor prefab based on weighted chances
        private FloorPrefab GetRandomFloorPrefab(FloorType floorType)
        {
            float totalChance = 0f;
            foreach (var prefab in floorType.floorPrefabs) totalChance += prefab.spawnChance;

            float randomValue = (float)_random.NextDouble() * totalChance;
            float cumulativeChance = 0f;

            foreach (var prefab in floorType.floorPrefabs)
            {
                cumulativeChance += prefab.spawnChance;
                if (randomValue < cumulativeChance) return prefab;
            }

            return null; // Fallback, though this should not happen
        }

        // Helper method to align the floor tile by its lowest point
        private void AlignFloorTile(GameObject floorTile)
        {
            // Get the parent room's Y position (assuming the room is the parent)
            float roomYPosition = transform.position.y; // Or floorTile.transform.parent.position.y if nested

            // Get the current Y position of the floor tile
            float currentYPosition = floorTile.transform.position.y;

            // Adjust the Y position of the floor tile to align with the room's Y position
            float adjustment = roomYPosition - currentYPosition;

            // Apply the adjustment to align the floor tile with the room's Y position
            floorTile.transform.position += new Vector3(0, adjustment, 0);

            //Debug.Log($"Aligned floor tile. Room Y: {roomYPosition}, Tile Y: {currentYPosition}, Adjustment: {adjustment}");
        }

        // Helper method to scale the floor tile to fit the room perfectly
        private void ScaleFloorTile(GameObject floorTile, Vector2 targetSize)
        {
            // Get the mesh size of the floor tile
            MeshFilter meshFilter = floorTile.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) return;

            Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
            Vector3 prefabScale = floorTile.transform.localScale;
            
            // Adjust mesh size according to the prefab's local scale
            Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale);

            // Calculate the scaling factor for the x and z dimensions
            float scaleX = targetSize.x / scaledMeshSize.x;
            float scaleZ = targetSize.y / scaledMeshSize.z;

            // Apply scaling to the floor tile, keeping the y scale unchanged
            floorTile.transform.localScale = new Vector3(scaleX * prefabScale.x, prefabScale.y, scaleZ * prefabScale.z);
        }
    }
}