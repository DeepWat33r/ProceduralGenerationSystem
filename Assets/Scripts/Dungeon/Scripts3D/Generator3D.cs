using System;
using System.Collections.Generic;
using Dungeon.Scripts2D;
using Graphs;
using Hallway;
using Room;
using UnityEngine;
using Random = System.Random;

namespace Dungeon.Scripts3D
{
    public partial class Generator3D : MonoBehaviour {
        public enum CellType {
            None,
            Room,
            Hallway,
            Stairs
        }

        class Room {
            public BoundsInt bounds;

            public Room(Vector3Int location, Vector3Int size) {
                bounds = new BoundsInt(location, size);
            }

            public static bool Intersect(Room a, Room b) {
                return !((a.bounds.position.x >= (b.bounds.position.x + b.bounds.size.x)) || ((a.bounds.position.x + a.bounds.size.x) <= b.bounds.position.x)
                    || (a.bounds.position.y >= (b.bounds.position.y + b.bounds.size.y)) || ((a.bounds.position.y + a.bounds.size.y) <= b.bounds.position.y)
                    || (a.bounds.position.z >= (b.bounds.position.z + b.bounds.size.z)) || ((a.bounds.position.z + a.bounds.size.z) <= b.bounds.position.z));
            }
        }

        [SerializeField]
        Vector3Int size;
        [SerializeField]
        int roomCount;
        [SerializeField]
        Vector3Int roomMinSize;
        [SerializeField]
        Vector3Int roomMaxSize;
        [SerializeField]
        GameObject cubePrefab;
        [SerializeField]
        Material redMaterial;
        [SerializeField]
        Material blueMaterial;
        [SerializeField]
        Material greenMaterial;

        Random random;
        Grid3D<CellType> grid;
        List<Room> rooms;
        Delaunay3D delaunay;
        HashSet<Prim.Edge> selectedEdges;
        Dictionary<Vector3Int, GameObject> hallwaysDictionary;
        Dictionary<Vector3Int, GameObject> roomsDictionary;
    
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private GameObject hallwayPrefab; 
        [SerializeField] private GameObject stairsPrefab; 
        void Start() {
            
            // Run the dungeon generation process synchronously
            Generate();
            InitializeAllHallways();
            InitializeAllRooms();
        }   
        private void Generate() {
            random = new Random((int)DateTime.Now.Ticks);  // Seed based on current time
            grid = new Grid3D<CellType>(size, Vector3Int.zero);
            rooms = new List<Room>();
            hallwaysDictionary = new Dictionary<Vector3Int, GameObject>();
            roomsDictionary = new Dictionary<Vector3Int, GameObject>();
            
            PlaceRooms();
            Triangulate();
            CreateHallways();
            PathfindHallways();
        }
        void PlaceRooms() {
            for (int i = 0; i < roomCount; i++) {
                Vector3Int location = new Vector3Int(
                    random.Next(0, size.x),
                    random.Next(0, size.y),
                    random.Next(0, size.z)
                );

                Vector3Int roomSize = new Vector3Int(
                    random.Next(roomMinSize.x, roomMaxSize.x + 1),
                    random.Next(roomMinSize.y, roomMaxSize.y + 1),
                    random.Next(roomMinSize.z, roomMaxSize.z + 1)
                );

                bool add = true;
                Room newRoom = new Room(location, roomSize);
                Room buffer = new Room(location + new Vector3Int(-1, 0, -1), roomSize + new Vector3Int(2, 0, 2));

                foreach (var room in rooms) {
                    if (Room.Intersect(room, buffer)) {
                        add = false;
                        break;
                    }
                }

                if (newRoom.bounds.xMin < 0 || newRoom.bounds.xMax >= size.x
                                            || newRoom.bounds.yMin < 0 || newRoom.bounds.yMax >= size.y
                                            || newRoom.bounds.zMin < 0 || newRoom.bounds.zMax >= size.z) {
                    add = false;
                }

                if (add) {
                    rooms.Add(newRoom);
                    PlaceRoom(newRoom.bounds.position, newRoom.bounds.size);

                    foreach (var pos in newRoom.bounds.allPositionsWithin) {
                        grid[pos] = CellType.Room;
                    }
                }
            }
        }

        void Triangulate() {
            List<Vertex> vertices = new List<Vertex>();

            foreach (var room in rooms) {
                vertices.Add(new Vertex<Room>((Vector3)room.bounds.position + ((Vector3)room.bounds.size) / 2, room));
            }

            delaunay = Delaunay3D.Triangulate(vertices);
        }

        void CreateHallways() {
            List<Prim.Edge> edges = new List<Prim.Edge>();

            foreach (var edge in delaunay.Edges) {
                edges.Add(new Prim.Edge(edge.U, edge.V));
            }

            List<Prim.Edge> minimumSpanningTree = Prim.MinimumSpanningTree(edges, edges[0].U);

            selectedEdges = new HashSet<Prim.Edge>(minimumSpanningTree);
            var remainingEdges = new HashSet<Prim.Edge>(edges);
            remainingEdges.ExceptWith(selectedEdges);

            foreach (var edge in remainingEdges) {
                if (random.NextDouble() < 0.125) {
                    selectedEdges.Add(edge);
                }
            }
        }

        void PathfindHallways() {
            DungeonPathfinder3D aStar = new DungeonPathfinder3D(size);

            foreach (var edge in selectedEdges) {
                var startRoom = (edge.U as Vertex<Room>).Item;
                var endRoom = (edge.V as Vertex<Room>).Item;

                var startPosf = startRoom.bounds.center;
                var endPosf = endRoom.bounds.center;
                var startPos = new Vector3Int((int)startPosf.x, (int)startPosf.y, (int)startPosf.z);
                var endPos = new Vector3Int((int)endPosf.x, (int)endPosf.y, (int)endPosf.z);

                var path = aStar.FindPath(startPos, endPos, (DungeonPathfinder3D.Node a, DungeonPathfinder3D.Node b) => {
                    var pathCost = new DungeonPathfinder3D.PathCost();

                    var delta = b.Position - a.Position;

                    if (delta.y == 0) {
                        //flat hallway
                        pathCost.cost = Vector3Int.Distance(b.Position, endPos);    //heuristic

                        if (grid[b.Position] == CellType.Stairs) {
                            return pathCost;
                        } else if (grid[b.Position] == CellType.Room) {
                            pathCost.cost += 5;
                        } else if (grid[b.Position] == CellType.None) {
                            pathCost.cost += 1;
                        }

                        pathCost.traversable = true;
                    } else {
                        //staircase
                        if ((grid[a.Position] != CellType.None && grid[a.Position] != CellType.Hallway)
                            || (grid[b.Position] != CellType.None && grid[b.Position] != CellType.Hallway)) return pathCost;

                        pathCost.cost = 100 + Vector3Int.Distance(b.Position, endPos);    //base cost + heuristic

                        int xDir = Mathf.Clamp(delta.x, -1, 1);
                        int zDir = Mathf.Clamp(delta.z, -1, 1);
                        Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                        Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);

                        if (!grid.InBounds(a.Position + verticalOffset)
                            || !grid.InBounds(a.Position + horizontalOffset)
                            || !grid.InBounds(a.Position + verticalOffset + horizontalOffset)) {
                            return pathCost;
                        }

                        if (grid[a.Position + horizontalOffset] != CellType.None
                            || grid[a.Position + horizontalOffset * 2] != CellType.None
                            || grid[a.Position + verticalOffset + horizontalOffset] != CellType.None
                            || grid[a.Position + verticalOffset + horizontalOffset * 2] != CellType.None) {
                            return pathCost;
                        }

                        pathCost.traversable = true;
                        pathCost.isStairs = true;
                    }

                    return pathCost;
                });

                if (path != null) {
                    for (int i = 0; i < path.Count; i++) {
                        var current = path[i];

                        if (grid[current] == CellType.None) {
                            grid[current] = CellType.Hallway;
                        }

                        if (i > 0) {
                            var prev = path[i - 1];

                            var delta = current - prev;

                            if (delta.y != 0) {
                                int xDir = Mathf.Clamp(delta.x, -1, 1);
                                int zDir = Mathf.Clamp(delta.z, -1, 1);
                                Vector3Int verticalOffset = new Vector3Int(0, delta.y, 0);
                                Vector3Int horizontalOffset = new Vector3Int(xDir, 0, zDir);
                            
                                grid[prev + horizontalOffset] = CellType.Stairs;
                                grid[prev + horizontalOffset * 2] = CellType.Stairs;
                                grid[prev + verticalOffset + horizontalOffset] = CellType.Stairs;
                                grid[prev + verticalOffset + horizontalOffset * 2] = CellType.Stairs;

                                PlaceStairs(prev+horizontalOffset, delta, horizontalOffset);
                            }

                            //Debug.DrawLine(prev + new Vector3(0.5f, 0.5f, 0.5f), current + new Vector3(0.5f, 0.5f, 0.5f), Color.blue, 100, false);
                        }
                    }

                    foreach (var pos in path) {
                        if (grid[pos] == CellType.Hallway) {
                            PlaceHallway(pos);
                        }
                    }
                }
            }
        }

        void PlaceCube(Vector3Int location, Vector3Int size, Material material) {
            GameObject go = Instantiate(cubePrefab, location, Quaternion.identity);
            go.GetComponent<Transform>().localScale = size;
            go.GetComponent<MeshRenderer>().material = material;
        }

        void PlaceRoom(Vector3Int location, Vector3Int size)
        {
            // Calculate the center position for the room
            Vector3 roomPosition = new Vector3(location.x + size.x / 2f, location.y, location.z + size.z / 2f);

            // Instantiate the room prefab at the calculated position
            GameObject roomObject = Instantiate(roomPrefab, roomPosition, Quaternion.identity);

            if (roomObject != null)
            {
                // Get the RoomGenerator component to set the size and generate contents
                RoomGenerator roomGenerator = roomObject.GetComponent<RoomGenerator>();

                if (roomGenerator != null)
                {
                    roomGenerator.roomSize = new Vector2(size.x, size.z);;

                    // Generate the room layout (walls, floors, etc.)
                    roomGenerator.GenerateRoom();

                    // Adjust position based on the calculated center
                    roomObject.transform.position = roomPosition;
                    
                    roomsDictionary.Add(location, roomObject);
                }
            }
        }
        void InitializeAllRooms()
        {
            foreach (var kvp in roomsDictionary)
            {
                var roomComponent = kvp.Value.GetComponent<RoomConnect3D>(); // Assuming RoomConnect is the equivalent component for rooms
                if (roomComponent != null)
                {
                    roomComponent.Initialize(grid); // Initialize each room with its grid position
                }
            }
        }
        
        void PlaceHallway(Vector3Int location)
        {
            // Check if the hallwayPrefab is assigned
            if (hallwayPrefab == null)
            {
                Debug.LogError("Hallway prefab is not assigned in the Inspector.");
                return;
            }
            // Ensure the dictionary is initialized
            if (hallwaysDictionary == null)
            {
                hallwaysDictionary = new Dictionary<Vector3Int, GameObject>();
            }
            // Check if the location already has a hallway
            if (!hallwaysDictionary.ContainsKey(location))
            {
                GameObject hallwayObj = Instantiate(hallwayPrefab, new Vector3(location.x + 0.5f, location.y, location.z + 0.5f), Quaternion.identity);
                hallwaysDictionary.Add(location, hallwayObj);
            }
        }

        void InitializeAllHallways()
        {
            foreach (var kvp in hallwaysDictionary)
            {
                // Get the HallwayConnect component from the hallway GameObject
                var hallwayComponent = kvp.Value.GetComponent<HallwayConnect3D>();
        
                if (hallwayComponent != null)
                {
                    // Initialize the hallway with the 3D grid and its specific 3D grid position
                    hallwayComponent.Initialize(grid, kvp.Key);
                }
            }
        }
        
        void PlaceStairs(Vector3Int location, Vector3Int delta, Vector3Int horizontalOffset)
        {
            // Adjust the y-coordinate based on delta to handle up and down placement
            int adjustedY = delta.y > 0 ? location.y : location.y - 1;

            // Calculate the position to match the former 4-cube configuration
            Vector3 stairPosition = new Vector3(
                location.x + horizontalOffset.x * 0.5f + 0.5f,
                adjustedY,
                location.z + horizontalOffset.z * 0.5f + 0.5f
            );

            // Determine the rotation based on the direction of horizontalOffset
            Quaternion stairRotation = Quaternion.identity;
    
            if (horizontalOffset.x == 1) 
                stairRotation = Quaternion.Euler(0, delta.y > 0 ? -90 : 90, 0); // East-facing
            else if (horizontalOffset.x == -1) 
                stairRotation = Quaternion.Euler(0, delta.y > 0 ? 90 : -90, 0); // West-facing
            else if (horizontalOffset.z == 1) 
                stairRotation = Quaternion.Euler(0, delta.y > 0 ? -180 : 0, 0); // North-facing
            else if (horizontalOffset.z == -1) 
                stairRotation = Quaternion.Euler(0, delta.y > 0 ? 0 : 180, 0); // South-facing

            // Instantiate the stair prefab with the calculated position and rotation
            Instantiate(stairsPrefab, stairPosition, stairRotation, transform);
        }






    }
}
