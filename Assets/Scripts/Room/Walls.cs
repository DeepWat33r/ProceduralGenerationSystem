using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Room
{
    [System.Serializable]
    public class WallType
    {
        public GameObject wallPrefab; 
        public float spawnChance;     
    }

    public class Walls : MonoBehaviour
    {
        public List<WallType> wallTypes; 
        public GameObject pillarPrefab;  
        private Random _random;    
        
        public void SetRandomSeed(int seed) => _random = new Random(seed); 

        public void GenerateWalls(Vector2 roomSize)
        {
            if (pillarPrefab == null) return;

            foreach (Transform child in transform)
                Destroy(child.gameObject);

            Vector3 basePosition = transform.position;

            CreateWall(basePosition + new Vector3(0, 0, roomSize.y / 2), Vector3.right, roomSize.x, "Wall_North", 180f); // North wall
            CreateWall(basePosition + new Vector3(0, 0, -roomSize.y / 2), Vector3.right, roomSize.x, "Wall_South"); // South wall
            CreateWall(basePosition + new Vector3(-roomSize.x / 2, 0, 0), Vector3.forward, roomSize.y, "Wall_West", 0f, true); // West wall
            CreateWall(basePosition + new Vector3(roomSize.x / 2, 0, 0), Vector3.forward, roomSize.y, "Wall_East", 180f, true); // East wall

            CreatePillar(basePosition + new Vector3(roomSize.x / 2, 0, roomSize.y / 2), pillarPrefab.transform.localScale);
            CreatePillar(basePosition + new Vector3(-roomSize.x / 2, 0, roomSize.y / 2), pillarPrefab.transform.localScale);
            CreatePillar(basePosition + new Vector3(roomSize.x / 2, 0, -roomSize.y / 2), pillarPrefab.transform.localScale);
            CreatePillar(basePosition + new Vector3(-roomSize.x / 2, 0, -roomSize.y / 2), pillarPrefab.transform.localScale);
        }
        
        private void CreateWall(Vector3 basePosition, Vector3 direction, float length, string wallTag, float additionalRotation = 0f, bool rotate = false)
        {
            WallType referenceWallType = GetRandomWallType();
            if (referenceWallType == null) return;

            MeshFilter referenceMeshFilter = referenceWallType.wallPrefab.GetComponentInChildren<MeshFilter>();
            if (referenceMeshFilter == null) return;

            Vector3 wallMeshSize = referenceMeshFilter.sharedMesh.bounds.size;
            Vector3 wallPrefabScale = referenceWallType.wallPrefab.transform.localScale;
            Vector3 scaledWallMeshSize = Vector3.Scale(wallMeshSize, wallPrefabScale);

            int wallCount = Mathf.Max(1, (int)(length / scaledWallMeshSize.x));
            float scale = (length / wallCount) / scaledWallMeshSize.x;

            for (int i = 0; i < wallCount; i++)
            {
                WallType selectedWallType = GetRandomWallType();
                if (selectedWallType == null) continue;

                Vector3 position = basePosition + direction * (-length / 2 + scaledWallMeshSize.x * scale / 2 + i * scale * scaledWallMeshSize.x);

                GameObject wall = Instantiate(selectedWallType.wallPrefab, position, Quaternion.identity, transform);
                wall.tag = wallTag;  // Set the tag based on the wall direction

                if (rotate) wall.transform.rotation = Quaternion.Euler(0, 90 + additionalRotation, 0);
                else wall.transform.rotation = Quaternion.Euler(0, additionalRotation, 0);

                wall.transform.localScale = new Vector3(scale * wallPrefabScale.x, wallPrefabScale.y, wallPrefabScale.z);
            }
        }

        private void CreatePillar(Vector3 position, Vector3 prefabScale)
        {
            if (pillarPrefab == null) return;

            GameObject pillar = Instantiate(pillarPrefab, position, Quaternion.identity, transform);
            pillar.transform.localScale = prefabScale;
        }

        private WallType GetRandomWallType()
        {
            if (wallTypes == null || wallTypes.Count == 0) return null;

            float totalChance = 0f;
            foreach (var wallType in wallTypes)
            {
                if (wallType.wallPrefab == null) continue;
                totalChance += wallType.spawnChance;
            }

            if (totalChance == 0f) return null;

            float randomValue = (float)_random.NextDouble() * totalChance; 
            float cumulativeChance = 0f;

            foreach (var wallType in wallTypes)
            {
                if (wallType.wallPrefab == null) continue;

                cumulativeChance += wallType.spawnChance;
                if (randomValue < cumulativeChance)
                {
                    return wallType;
                }
            }

            return null;
        }
    }
}
