using Room;
using UnityEngine;

namespace Hallway
{
    public class HallwayGenerator : MonoBehaviour
    {
        public Vector2 hallwaySize = new Vector2(1, 1);
        public int randomSeed = 12345;

        [SerializeField] private Walls walls;
        [SerializeField] private Floor floor;
        [SerializeField] private Ceiling ceiling;
        
        void Start()
        {
            GenerateHallway();
        }

        private void GenerateHallway()
        {
            GetComponents();
            walls.GenerateWalls(hallwaySize);
            floor.GenerateFloor(hallwaySize);
            ceiling.GenerateCeiling(hallwaySize);
        }
        private void GetComponents()
        {
            walls = GetComponent<Walls>();
            if (walls == null) Debug.LogError("Walls component is missing on the GameObject.");
            walls.SetRandomSeed(randomSeed);

            floor = GetComponent<Floor>();
            if (floor == null) Debug.LogError("Floor component is missing on the GameObject.");
            floor.SetRandomSeed(randomSeed);

            ceiling = GetComponent<Ceiling>();
            if (ceiling == null) Debug.LogError("Ceiling component is missing on the GameObject.");
        }
    }
}
