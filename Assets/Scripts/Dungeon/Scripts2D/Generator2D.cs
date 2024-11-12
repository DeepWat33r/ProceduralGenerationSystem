using System;
using System.Collections.Generic;
using Graphs;
using Hallway;
using Room;
using UnityEngine;
using Random = System.Random;

namespace Dungeon.Scripts2D
{
    public partial class Generator2D : MonoBehaviour
    {
        public enum CellType
        {
            None,
            Room,
            Hallway
        }

        class Room
        {
            public RectInt bounds;

            public Room(Vector2Int location, Vector2Int size)
            {
                bounds = new RectInt(location, size);
            }

            public static bool Intersect(Room a, Room b)
            {
                return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) ||
                         ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x) ||
                         (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) ||
                         ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y));
            }
        }

        [SerializeField] Vector2Int size;
        [SerializeField] int roomCount;
        [SerializeField] Vector2Int roomMinSize;
        [SerializeField] Vector2Int roomMaxSize;
        [SerializeField] GameObject cubePrefab;
        [SerializeField] Material redMaterial;
        [SerializeField] Material blueMaterial;

        Random random;
        Grid2D<CellType> grid;
        List<Room> rooms;
        Dictionary<Vector2Int, GameObject> hallwaysDictionary;
        Dictionary<Vector2Int, GameObject> roomsDictionary;
        Delaunay2D delaunay;
        HashSet<Prim.Edge> selectedEdges;

        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject hallwayPrefab;

        void Start()
        {
            Generate();
            InitializeAllHallways();
            InitializeAllRooms();
        }

        void Generate()
        {
            random = new Random((int)DateTime.Now.Ticks);
            grid = new Grid2D<CellType>(size, Vector2Int.zero);
            rooms = new List<Room>();
            hallwaysDictionary = new Dictionary<Vector2Int, GameObject>();
            roomsDictionary = new Dictionary<Vector2Int, GameObject>();
            
            PlaceRooms();
            Triangulate();
            CreateHallways();
            PathfindHallways();
        }

        void PlaceRooms()
        {
            for (int i = 0; i < roomCount; i++)
            {
                Vector2Int location = new Vector2Int(
                    random.Next(0, size.x),
                    random.Next(0, size.y)
                );

                Vector2Int roomSize = new Vector2Int(
                    random.Next(roomMinSize.x, roomMaxSize.x + 1),
                    random.Next(roomMinSize.y, roomMaxSize.y + 1)
                );

                Room newRoom = new Room(location, roomSize);
                Room buffer = new Room(location + new Vector2Int(-1, -1), roomSize + new Vector2Int(2, 2));
                bool add = true;

                foreach (var room in rooms)
                {
                    if (Room.Intersect(room, buffer))
                    {
                        add = false;
                        break;
                    }
                }

                if (add && newRoom.bounds.xMin >= 0 && newRoom.bounds.xMax < size.x &&
                    newRoom.bounds.yMin >= 0 && newRoom.bounds.yMax < size.y)
                {
                    rooms.Add(newRoom);
                    PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                    foreach (var pos in newRoom.bounds.allPositionsWithin)
                    {
                        grid[pos] = CellType.Room;
                    }
                }
            }
        }

        void Triangulate()
        {
            List<Vertex> vertices = new List<Vertex>();

            foreach (var room in rooms)
            {
                vertices.Add(new Vertex<Room>((Vector2)room.bounds.position + ((Vector2)room.bounds.size) / 2, room));
            }

            delaunay = Delaunay2D.Triangulate(vertices);
        }

        void CreateHallways()
        {
            List<Prim.Edge> edges = new List<Prim.Edge>();

            foreach (var edge in delaunay.Edges)
            {
                edges.Add(new Prim.Edge(edge.U, edge.V));
            }

            List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);
            selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
            var remainingEdges = new HashSet<Prim.Edge>(edges);
            remainingEdges.ExceptWith(selectedEdges);

            foreach (var edge in remainingEdges)
            {
                if (random.NextDouble() < 0.125)
                {
                    selectedEdges.Add(edge);
                }
            }
        }

        void PathfindHallways()
        {
            DungeonPathfinder2D aStar = new DungeonPathfinder2D(size);

            foreach (var edge in selectedEdges)
            {
                var startRoom = (edge.U as Vertex<Room>).Item;
                var endRoom = (edge.V as Vertex<Room>).Item;

                var startPos = new Vector2Int((int)startRoom.bounds.center.x, (int)startRoom.bounds.center.y);
                var endPos = new Vector2Int((int)endRoom.bounds.center.x, (int)endRoom.bounds.center.y);

                var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder2D.Node a, DungeonPathfinder2D.Node b) => {
                    var pathCost = new DungeonPathfinder2D.PathCost
                    {
                        cost = Vector2Int.Distance(b.Position, endPos), // heuristic
                        traversable = true
                    };

                    if (grid[b.Position] == CellType.Room)
                    {
                        pathCost.cost += 10;
                    }
                    else if (grid[b.Position] == CellType.None)
                    {
                        pathCost.cost += 5;
                    }
                    else if (grid[b.Position] == CellType.Hallway)
                    {
                        pathCost.cost += 1;
                    }

                    return pathCost;
                });

                if (path != null)
                {
                    foreach (var pos in path)
                    {
                        if (grid[pos] == CellType.None)
                        {
                            grid[pos] = CellType.Hallway;
                            PlaceHallway(pos);
                        }
                    }
                }
            }
        }
        
        void PlaceRoom(Vector2Int location, Vector2Int size)
        {
            Vector3 roomPosition = new Vector3(location.x, 0, location.y);
            GameObject roomObject = Instantiate(roomPrefab, roomPosition, Quaternion.identity);

            if (roomObject != null)
            {
                // Get the RoomGenerator component from the instantiated room object
                RoomGenerator roomGenerator = roomObject.GetComponent<RoomGenerator>();

                if (roomGenerator != null)
                {
                    // Set the room size directly on the RoomGenerator component
                    Vector2 adjustedRoomSize = new Vector2(size.x, size.y);
                    roomGenerator.roomSize = adjustedRoomSize;

                    // Generate the room contents based on the specified size
                    roomGenerator.GenerateRoom();

                    // Adjust the room position to center it within the grid cell based on its size
                    Vector3 adjustedPosition = new Vector3(
                        roomPosition.x + (adjustedRoomSize.x / 2f),
                        roomPosition.y,
                        roomPosition.z + (adjustedRoomSize.y / 2f)
                    );
                    roomObject.transform.position = adjustedPosition;

                    // Add the room to the dictionary for later reference
                    roomsDictionary.Add(location, roomObject);
                }
            }
        }
        void InitializeAllRooms()
        {
            foreach (var kvp in roomsDictionary)
            {
                var roomComponent = kvp.Value.GetComponent<RoomConnect>(); // Assuming RoomConnect is the equivalent component for rooms
                if (roomComponent != null)
                {
                    roomComponent.Initialize(grid); // Initialize each room with its grid position
                }
            }
        }
        
        void PlaceHallway(Vector2Int location)
        {
            GameObject hallwayObj = Instantiate(hallwayPrefab, new Vector3(location.x + 0.5f, 0, location.y + 0.5f), Quaternion.identity);
            hallwaysDictionary.Add(location, hallwayObj);
        }

        void InitializeAllHallways()
        {
            foreach (var kvp in hallwaysDictionary)
            {
                var hallwayComponent = kvp.Value.GetComponent<HallwayConnect>();
                if (hallwayComponent != null)
                {
                    hallwayComponent.Initialize(grid, kvp.Key);
                }
            }
        }
    }
}
