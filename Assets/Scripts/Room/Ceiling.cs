using UnityEngine;

namespace Room
{
    public class Ceiling : MonoBehaviour
    {
        public GameObject ceilingPrefab; 
        [SerializeField] private Walls wallsScript; 
        public void GenerateCeiling(Vector2 roomSize)
        {
            Vector3 basePosition = transform.position;

            MeshFilter meshFilter = ceilingPrefab.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) { Debug.LogError("Ceiling prefab does not have a MeshFilter component."); return; }

            Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
            Vector3 prefabScale = ceilingPrefab.transform.localScale;
            Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale);

            int tileCountX = Mathf.CeilToInt(roomSize.x / scaledMeshSize.x);
            int tileCountZ = Mathf.CeilToInt(roomSize.y / scaledMeshSize.z);

            Vector2 tileSize = new Vector2(roomSize.x / tileCountX, roomSize.y / tileCountZ);

            float ceilingHeight = GetWallHeight()-0.1f; 

            for (int x = 0; x < tileCountX; x++)
            {
                for (int z = 0; z < tileCountZ; z++)
                {
                    Vector3 position = basePosition + new Vector3(x * tileSize.x - roomSize.x / 2f + tileSize.x / 2f, ceilingHeight, z * tileSize.y - roomSize.y / 2f + tileSize.y / 2f);

                    GameObject ceilingTile = Instantiate(ceilingPrefab, position, Quaternion.identity, transform);

                    ScaleCeilingTile(ceilingTile, tileSize);

                    //Debug.Log($"Ceiling Tile Position: {position}");
                }
            }
        }

        private void ScaleCeilingTile(GameObject ceilingTile, Vector2 targetSize)
        {
            MeshFilter meshFilter = ceilingTile.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) return;

            Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
            Vector3 prefabScale = ceilingTile.transform.localScale;

            Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale);

            float scaleX = targetSize.x / scaledMeshSize.x;
            float scaleZ = targetSize.y / scaledMeshSize.z;

            ceilingTile.transform.localScale = new Vector3(scaleX * prefabScale.x, prefabScale.y, scaleZ * prefabScale.z);
        }
        private float GetWallHeight()
        {
            if (wallsScript == null) { Debug.LogError("Walls script reference is missing. Cannot determine ceiling height."); return 4f; }

            WallType sampleWallType = wallsScript.wallTypes[0];  
            GameObject wallPrefab = sampleWallType.wallPrefab;
            MeshFilter meshFilter = wallPrefab.GetComponentInChildren<MeshFilter>();

            if (meshFilter == null) { Debug.LogError("Wall prefab does not have a MeshFilter component."); return 4f; }

            Vector3 wallSize = meshFilter.sharedMesh.bounds.size;
            Vector3 wallScale = wallPrefab.transform.localScale;
            float wallHeight = wallSize.y * wallScale.y;

            return wallHeight; 
        }
    }
}
