using UnityEngine;
using Room.Grid;

namespace Room
{
    public class RoomGenerator : MonoBehaviour
    {
        public Vector2 roomSize = new Vector2(10, 10);
        public int randomSeed = 12345;

        [SerializeField] private Walls walls;
        [SerializeField] private Floor floor;
        [SerializeField] private Ceiling ceiling;
        private GridManager gridManager;
        private DecorationManager decorationManager;

        private Vector2 previousRoomSize;

        void Start()
        {
            //GetComponents();
            GenerateRoom();
            previousRoomSize = roomSize;
        }

        void Update()
        {
            if (roomSize != previousRoomSize)
            {
                GenerateRoom();
                previousRoomSize = roomSize;
            }
        }

        public void GenerateRoom()
        {
            GetComponents();
            walls.GenerateWalls(roomSize);
            floor.GenerateFloor(roomSize);
            ceiling.GenerateCeiling(roomSize);

            gridManager.CreateGrid();

            decorationManager.SetRandomSeed(randomSeed);
            decorationManager.PopulateGridWithDecorations();
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

            gridManager = GetComponent<GridManager>();
            if (gridManager == null) Debug.LogError("GridManager component is missing on the GameObject.");

            decorationManager = GetComponent<DecorationManager>();
            if (decorationManager == null) Debug.LogError("DecorationManager component is missing on the GameObject.");
            decorationManager.SetRandomSeed(randomSeed);
        }
    }
}
