// using UnityEngine;
//
// namespace Hallway
// {
//     public class HallwayScanner : MonoBehaviour
//     {
//         public LayerMask roomLayerMask;    // Layer mask to detect rooms
//         public LayerMask hallwayLayerMask; // Layer mask to detect hallways
//         public LayerMask wallLayerMask;    // Layer mask to detect walls
//         public Color rayColor = Color.red; // Color for the ray visualization
//         public float rayLength = 1f;       // Set to 1f for scanning distance
//
//         public void ScanForAdjacentHallwaysOrRooms(GameObject hallwayObject)
//         {
//             // Get the position of the hallway and adjust Y by 0.5f to account for wall height
//             Vector3 hallwayPosition = hallwayObject.transform.position + new Vector3(0, 0.5f, 0);
//
//             // Scan in all four directions
//             ScanDirection(hallwayObject, hallwayPosition, Vector3.right, "East");   // Check to the right (East)
//             ScanDirection(hallwayObject, hallwayPosition, Vector3.left, "West");    // Check to the left (West)
//             ScanDirection(hallwayObject, hallwayPosition, Vector3.forward, "North"); // Check forward (North)
//             ScanDirection(hallwayObject, hallwayPosition, Vector3.back, "South");    // Check backward (South)
//         }
//
//         private void ScanDirection(GameObject hallwayObject, Vector3 startPosition, Vector3 direction, string directionName)
//         {
//             Debug.Log($"Scanning {directionName} direction with ray length {rayLength}");
//
//             // Cast a ray in the given direction to check for adjacent hallways or rooms
//             Ray ray = new Ray(startPosition, direction);
//             RaycastHit[] hits = Physics.RaycastAll(ray, rayLength, wallLayerMask | roomLayerMask | hallwayLayerMask, QueryTriggerInteraction.Ignore);
//
//             // Draw the ray in the Scene View for visualization
//             //Debug.DrawRay(startPosition, direction * rayLength, rayColor, 1f);
//
//             if (hits.Length == 0)
//             {
//                 Debug.Log($"No object detected in {directionName} direction.");
//                 return;
//             }
//
//             // Process each hit along the ray
//             foreach (var hit in hits)
//             {
//                 GameObject hitObject = hit.collider.gameObject;
//
//                 // Ignore walls that are part of the same hallway (or child objects of the same hallway)
//                 if (hitObject.transform.IsChildOf(hallwayObject.transform))
//                 {
//                     Debug.Log($"Ignored wall from the same hallway at {hitObject.transform.position} in {directionName} direction");
//                     continue;
//                 }
//
//                 // If the hit object is a wall
//                 if (IsInLayer(hitObject, wallLayerMask))
//                 {
//                     Debug.Log($"Detected a wall at {hitObject.transform.position} in {directionName} direction");
//
//                     // Check if the parent is in the hallway layer
//                     GameObject parentObject = hitObject.transform.parent != null ? hitObject.transform.parent.gameObject : hitObject;
//                     if (IsInLayer(parentObject, hallwayLayerMask))
//                     {
//                         // Found an adjacent hallway, so remove the wall in both hallways
//                         Debug.Log($"Detected an adjacent hallway at {parentObject.transform.position} in {directionName} direction.");
//
//                         // Remove the wall from the adjacent hallway
//                         Destroy(hitObject);
//                         Debug.Log($"Destroyed wall in adjacent hallway at {hitObject.transform.position} in {directionName} direction");
//
//                         // Remove the wall from the current hallway
//                         RemoveWall(hallwayObject, direction);
//                     }
//                 }
//             }
//         }
//
//         private void RemoveWall(GameObject hallwayObject, Vector3 direction)
//         {
//             // Find the correct wall by checking its relative position to the hallway's center
//             Vector3 hallwayPosition = hallwayObject.transform.position;
//
//             // Loop through all child objects to find the correct wall in the direction
//             foreach (Transform child in hallwayObject.transform)
//             {
//                 if (child.CompareTag("Wall"))
//                 {
//                     // Calculate the relative position of the wall to the hallway center
//                     Vector3 wallPosition = child.transform.position - hallwayPosition;
//
//                     // Determine if this wall is facing the correct direction based on the direction we are scanning
//                     if (IsWallInDirection(wallPosition, direction))
//                     {
//                         // Destroy the wall that matches the direction of the adjacent hallway
//                         Destroy(child.gameObject);
//                         Debug.Log($"Destroyed wall facing {GetDirectionName(direction)} in current hallway at {child.position}");
//                         return; // Stop after destroying the correct wall
//                     }
//                 }
//             }
//         }
//
//         private bool IsWallInDirection(Vector3 wallPosition, Vector3 direction)
//         {
//             // Normalize the position to avoid precision errors (use an epsilon value to compare)
//             const float epsilon = 0.1f;
//
//             // We use the direction vector to check if the wall is in the expected position
//             if (direction == Vector3.right && Mathf.Abs(wallPosition.x - 0.5f) < epsilon) return true;   // East
//             if (direction == Vector3.left && Mathf.Abs(wallPosition.x + 0.5f) < epsilon) return true;    // West
//             if (direction == Vector3.forward && Mathf.Abs(wallPosition.z - 0.5f) < epsilon) return true; // North
//             if (direction == Vector3.back && Mathf.Abs(wallPosition.z + 0.5f) < epsilon) return true;    // South
//
//             return false;
//         }
//
//         // Helper method to check if an object is in a specific layer
//         private bool IsInLayer(GameObject obj, LayerMask layerMask)
//         {
//             return ((layerMask & (1 << obj.layer)) != 0);
//         }
//
//         private string GetDirectionName(Vector3 direction)
//         {
//             if (direction == Vector3.right) return "East";
//             if (direction == Vector3.left) return "West";
//             if (direction == Vector3.forward) return "North";
//             if (direction == Vector3.back) return "South";
//             return "Unknown";
//         }
//
//         // Optional: Visualize the rays in the Scene view
//         private void OnDrawGizmos()
//         {
//             Gizmos.color = rayColor;
//             Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.right * rayLength);
//             Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.left * rayLength);
//             Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.forward * rayLength);
//             Gizmos.DrawRay(transform.position + new Vector3(0, 0.5f, 0), Vector3.back * rayLength);
//         }
//     }
// }
