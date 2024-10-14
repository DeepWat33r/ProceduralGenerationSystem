using Room;
using UnityEngine;

namespace Dungeon.Scripts2D
{
    public class RoomReplacer : MonoBehaviour
    {
        public void ReplaceCubesWithRooms(GameObject roomPrefab, Material roomCubeMaterial)
        {
            //Debug.Log("Starting ReplaceCubesWithRooms");  // Debug the start of the process

            // Find all cubes in the scene
            GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");
            //Debug.Log($"Found {allCubes.Length} cubes in the scene.");

            foreach (GameObject cube in allCubes)
            {
                // Check if the cube has the material we are looking for
                MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                {
                    //Debug.Log($"Cube at position {cube.transform.position} has material {meshRenderer.sharedMaterial.name}");

                    if (meshRenderer.sharedMaterial == roomCubeMaterial)
                    {
                        //Debug.Log($"Identified cube for replacement at {cube.transform.position}");

                        // Cube is identified as a room cube based on the material
                        Vector3 cubePosition = cube.transform.position;
                        Vector3 cubeScale = cube.transform.localScale;

                        // Instantiate the room prefab at the cube's position
                        GameObject roomObject = Instantiate(roomPrefab, cubePosition, Quaternion.identity);

                        if (roomObject != null)
                        {
                            //Debug.Log("Room prefab instantiated successfully.");

                            RoomGenerator roomGenerator = roomObject.GetComponent<RoomGenerator>();

                            if (roomGenerator != null)
                            {
                                // Adjust room size according to the cube's size (4x cube size)
                                Vector2 adjustedRoomSize = new Vector2(cubeScale.x , cubeScale.z );
                                roomGenerator.roomSize = adjustedRoomSize;
                                //Debug.Log($"Room size set to {adjustedRoomSize}");

                                roomGenerator.GenerateRoom();
                                //Debug.Log("Room generation called.");

                                // Scale the room to 0.25
                                //roomObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                                //Debug.Log("Room scaled to 0.25.");

                                // Adjust position (centered room adjustment)
                                Vector3 adjustedPosition = new Vector3(
                                    cubePosition.x + (adjustedRoomSize.x / 2f),
                                    cubePosition.y,
                                    cubePosition.z + (adjustedRoomSize.y / 2f)
                                );
                                roomObject.transform.position = adjustedPosition;
                                //Debug.Log($"Room position set to {adjustedPosition}");

                                // Destroy the cube after replacing it with the room
                                Destroy(cube);
                                //Debug.Log("Cube destroyed.");
                            }
                        }
                    }
                }
            }
        }
        public void ReplaceCubesWithHallways(GameObject hallwayPrefab, Material hallwayCubeMaterial)
        {
            // Find all cubes in the scene
            GameObject[] allCubes = GameObject.FindGameObjectsWithTag("Cube");

            foreach (GameObject cube in allCubes)
            {
                // Check if the cube has the material we are looking for
                MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();

                if (meshRenderer != null && meshRenderer.sharedMaterial == hallwayCubeMaterial)
                {
                    // Cube is identified as a hallway cube based on the material
                    Vector3 cubePosition = cube.transform.position;
                    Vector3 cubeScale = cube.transform.localScale;

                    // Instantiate the hallway prefab at the cube's position
                    GameObject hallwayObject = Instantiate(hallwayPrefab, cubePosition, Quaternion.identity);

                    if (hallwayObject != null)
                    {
                        // Center the hallway to the cube's center (since the cube’s center is offset)
                        Vector3 adjustedPosition = new Vector3(
                            cubePosition.x + (cubeScale.x / 2f),  // Adjust for X center
                            cubePosition.y,                      // Keep Y position the same
                            cubePosition.z + (cubeScale.z / 2f)   // Adjust for Z center
                        );
                        hallwayObject.transform.position = adjustedPosition;

                        // Destroy the cube after replacing it with the hallway
                        Destroy(cube);
                    }
                }
            }
        }
    }
}
