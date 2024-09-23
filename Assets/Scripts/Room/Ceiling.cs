using UnityEngine;

namespace Room
{
    public class Ceiling : MonoBehaviour
    {
        public GameObject ceilingPrefab;  // Assign the ceiling prefab here
        [SerializeField] private Walls wallsScript; // Reference to Walls script
        // Method to generate the ceiling based on room size
        public void GenerateCeiling(Vector2 roomSize)
        {
            // Use the parent GameObject's position as the base position
            Vector3 basePosition = transform.position;

            // Get the mesh size from the ceiling prefab to calculate the number of tiles needed
            MeshFilter meshFilter = ceilingPrefab.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) { Debug.LogError("Ceiling prefab does not have a MeshFilter component."); return; }

            Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
            Vector3 prefabScale = ceilingPrefab.transform.localScale;
            Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale); // Adjust mesh size according to the prefab's local scale

            // Calculate the number of tiles needed based on room size and scaled tile size
            int tileCountX = Mathf.CeilToInt(roomSize.x / scaledMeshSize.x);
            int tileCountZ = Mathf.CeilToInt(roomSize.y / scaledMeshSize.z);

            // Calculate the adjusted tile size to fit the room perfectly
            Vector2 tileSize = new Vector2(roomSize.x / tileCountX, roomSize.y / tileCountZ);

            // Set the ceiling height above the room (you can adjust this value as needed)
            float ceilingHeight = GetWallHeight(); 

            // Generate the ceiling tiles
            for (int x = 0; x < tileCountX; x++)
            {
                for (int z = 0; z < tileCountZ; z++)
                {
                    // Calculate the position for each ceiling tile, centering in the room
                    Vector3 position = basePosition + new Vector3(x * tileSize.x - roomSize.x / 2f + tileSize.x / 2f, ceilingHeight, z * tileSize.y - roomSize.y / 2f + tileSize.y / 2f);

                    // Instantiate the ceiling tile
                    GameObject ceilingTile = Instantiate(ceilingPrefab, position, Quaternion.identity, transform);

                    // Scale the ceiling tile to fit the room perfectly
                    ScaleCeilingTile(ceilingTile, tileSize);

                    // Log the position for debugging
                    //Debug.Log($"Ceiling Tile Position: {position}");
                }
            }
        }

        // Helper method to scale the ceiling tile to fit the room perfectly
        private void ScaleCeilingTile(GameObject ceilingTile, Vector2 targetSize)
        {
            // Get the mesh size of the ceiling tile
            MeshFilter meshFilter = ceilingTile.GetComponentInChildren<MeshFilter>();
            if (meshFilter == null) return;

            Vector3 meshSize = meshFilter.sharedMesh.bounds.size;
            Vector3 prefabScale = ceilingTile.transform.localScale;

            // Adjust mesh size according to the prefab's local scale
            Vector3 scaledMeshSize = Vector3.Scale(meshSize, prefabScale);

            // Calculate the scaling factor for the x and z dimensions
            float scaleX = targetSize.x / scaledMeshSize.x;
            float scaleZ = targetSize.y / scaledMeshSize.z;

            // Apply scaling to the ceiling tile, keeping the y scale unchanged
            ceilingTile.transform.localScale = new Vector3(scaleX * prefabScale.x, prefabScale.y, scaleZ * prefabScale.z);
        }
        private float GetWallHeight()
        {
            if (wallsScript == null) { Debug.LogError("Walls script reference is missing. Cannot determine ceiling height."); return 4f; }

            // Assume walls are uniform in height, get height from one of the wall types
            WallType sampleWallType = wallsScript.wallTypes[0];  
            GameObject wallPrefab = sampleWallType.wallPrefab;
            MeshFilter meshFilter = wallPrefab.GetComponentInChildren<MeshFilter>();

            if (meshFilter == null) { Debug.LogError("Wall prefab does not have a MeshFilter component."); return 4f; }

            // Calculate the wall height considering its scale
            Vector3 wallSize = meshFilter.sharedMesh.bounds.size;
            Vector3 wallScale = wallPrefab.transform.localScale;
            float wallHeight = wallSize.y * wallScale.y;

            return wallHeight; // Set ceiling at the top of the walls
        }
    }
}
