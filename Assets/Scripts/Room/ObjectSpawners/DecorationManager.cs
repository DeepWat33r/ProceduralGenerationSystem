// using Room.Grid;
// using UnityEngine;
//
// namespace Room.ObjectSpawners
// {
//     public class DecorationManager : MonoBehaviour
//     {
//         private GridManager _gridManager;
//
//         public void Initialize(GridManager gridManager)
//         {
//             _gridManager = gridManager;
//
//             // Find all Spawner components attached to this GameObject
//             Spawner[] spawners = GetComponents<Spawner>();
//
//             foreach (Spawner spawner in spawners)
//             {
//                 spawner.Spawn(gridManager);
//             }
//         }
//     }
// }