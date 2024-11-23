using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Room
{
    [System.Serializable]
    public class FloorPrefab
    {
        public GameObject floorPrefab; 
        public float spawnChance;     
    }

    [System.Serializable]
    public class FloorType
    {
        public string typeName;                
        public List<FloorPrefab> floorPrefabs;  
    }
    public class Floor : MonoBehaviour
    {
        public List<FloorType> floorTypes;
        private Random _random;    
        
        public void SetRandomSeed(int seed) => _random = new Random(seed); 


        public void GenerateFloor(Vector2 roomSize)
        {
            Vector3 basePosition = transform.position;

            int tileCountX = Mathf.CeilToInt(roomSize.x / 0.5f);
            int tileCountZ = Mathf.CeilToInt(roomSize.y / 0.5f);

            FloorType selectedFloorType = GetRandomFloorType();
            if (selectedFloorType == null) { Debug.LogError("Failed to select a floor type."); return; }

            Vector2 tileSize = new Vector2(roomSize.x / tileCountX, roomSize.y / tileCountZ);

            for (int x = 0; x < tileCountX; x++)
            {
                for (int z = 0; z < tileCountZ; z++)
                {
                    Vector3 position = basePosition + new Vector3(x * tileSize.x - roomSize.x / 2f + tileSize.x / 2f, 0, z * tileSize.y - roomSize.y / 2f + tileSize.y / 2f);

                    FloorPrefab selectedFloorPrefab = GetRandomFloorPrefab(selectedFloorType);
                    if (selectedFloorPrefab == null) continue;

                    GameObject floorTile = Instantiate(selectedFloorPrefab.floorPrefab, position, Quaternion.identity, transform);

                    AlignFloorTile(floorTile);

                    ScaleFloorTile(floorTile, tileSize);

                    //Debug.Log($"Floor Tile Position: {position}, Type: {selectedFloorType.typeName}");
                }
            }
        }

        private FloorType GetRandomFloorType()
        {
            if (floorTypes == null || floorTypes.Count == 0) { Debug.LogError("No floor types available for selection."); return null; }

            int selectedIndex = _random.Next(floorTypes.Count);
            return floorTypes[selectedIndex];
        }

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

            return null; 
        }

        private void AlignFloorTile(GameObject floorTile)
        {
            float roomYPosition = transform.position.y; 

            float currentYPosition = floorTile.transform.position.y;

            float adjustment = roomYPosition - currentYPosition;

            floorTile.transform.position += new Vector3(0, adjustment, 0);

            //Debug.Log($"Aligned floor tile. Room Y: {roomYPosition}, Tile Y: {currentYPosition}, Adjustment: {adjustment}");
        }

        private void ScaleFloorTile(GameObject floorTile, Vector2 targetSize)
        {
            MeshFilter meshFilter = floorTile.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) return;

            Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
            Vector3 prefabScale = floorTile.transform.localScale;
            
            Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale);

            float scaleX = targetSize.x / scaledMeshSize.x;
            float scaleZ = targetSize.y / scaledMeshSize.z;

            floorTile.transform.localScale = new Vector3(scaleX * prefabScale.x, prefabScale.y, scaleZ * prefabScale.z);
        }
    }
}