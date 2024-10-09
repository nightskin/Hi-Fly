using System;
using System.Collections.Generic;
using UnityEngine;

enum LevelGenerationAlgorithm
{
    RANDOM_WALKER_2D,
    RANDOM_WALKER_3D,
    TINY_KEEP
}
class GridPoint
{
    public Vector3 position;
    public bool empty;

    public GridPoint()
    {
        position = Vector3.zero;
        empty = false;
    }

    public GridPoint(bool e)
    {
        empty = e;
        position = Vector3.zero;
    }

    public static int GetState(GridPoint[] points)
    {
        int state = 0;
        if (points[0].empty) state |= 1;
        if (points[1].empty) state |= 2;
        if (points[2].empty) state |= 4;
        if (points[3].empty) state |= 8;
        if (points[4].empty) state |= 16;
        if (points[5].empty) state |= 32;
        if (points[6].empty) state |= 64;
        if (points[7].empty) state |= 128;
        return state;
    }

    public static Vector3 GetMidPoint(GridPoint point1, GridPoint point2)
    {
        return (point1.position + point2.position) / 2;
    }

}
struct Room
{
    public Vector3Int indexPosition;
    public Vector3Int indexSize;
    public Vector3Int[] exits;

    public Room(Vector3Int indexPosition, Vector3Int indexSize)
    {
        this.indexPosition = indexPosition;
        this.indexSize = indexSize;
        exits = new Vector3Int[4];
        exits[0] = indexPosition + new Vector3Int(indexSize.x, 0, 0);
        exits[1] = indexPosition + new Vector3Int(-indexSize.x, 0, 0);
        exits[2] = indexPosition + new Vector3Int(0, 0, indexSize.z);
        exits[3] = indexPosition + new Vector3Int(0, 0, -indexSize.z);
    }

    public Vector3Int GetNearestExit(Vector3Int toIndex)
    {
        Vector3Int nearest = exits[0];
        for (int i = 1; i < exits.Length; i++)
        {
            if (Vector3Int.Distance(toIndex, exits[i]) < Vector3Int.Distance(toIndex, nearest))
            {
                nearest = exits[i];
            }
        }
        return nearest;
    }

}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class DungeonMesh : MonoBehaviour
{
    [Header("Default Parameters")]
    [Tooltip("Player GameObject That will be placed in the level on Runtime")] public Transform player;
    
    [Tooltip("Determines max size of the level")][Min(3)] public Vector3Int gridSize = Vector3Int.one * 100;
    [Tooltip("Controls how far apart everything is")][Min(1)] public float tileSize = 10;
    public string seed = string.Empty;
    [SerializeField] LevelGenerationAlgorithm LevelGenerationAlgorithm;

    GridPoint[,,] grid = null;
    List<Vector3> verts;
    List<Vector2> uvs;
    List<int> tris;
    int buffer;
    Mesh mesh;

    [Space]
    [Header("RANDOM_WALKER Parameters")]
    [SerializeField][Min(1)] int numberOfSteps = 200;

    [Space]
    [Header("TINY_KEEP Parameters")]
    [SerializeField][Min(2)] int numberOfRooms = 2;
    [SerializeField][Min(1)] int hallwaySize = 1;
    [SerializeField][Min(1)] int roomHeight = 1;
    [SerializeField][Min(1)] int minRoomSize = 2;
    [SerializeField][Min(1)] int maxRoomSize = 10;

    Room[] rooms;
    
    void Start()
    {
        Init();
        PopulateValues(LevelGenerationAlgorithm);
        GenerateMesh();

        PlacePlayer();

    }
    
    void PlacePlayer()
    {
        if (grid == null) return;
        if(!player) return;
        else
        {
            if (LevelGenerationAlgorithm == LevelGenerationAlgorithm.RANDOM_WALKER_3D)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int y = 0; y < gridSize.y; y++)
                    {
                        for (int z = 0; z < gridSize.z; z++)
                        {
                            if (grid[x, y, z].empty)
                            {
                                player.position = grid[x, y, z].position;
                            }
                        }
                    }
                }
            }
            else if (LevelGenerationAlgorithm == LevelGenerationAlgorithm.RANDOM_WALKER_2D)
            {
                for (int x = 0; x < gridSize.x; x++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        if (grid[x, 0, z].empty)
                        {
                            player.position = grid[x, 0, z].position;
                        }
                    }
                }
            }
            else if (LevelGenerationAlgorithm == LevelGenerationAlgorithm.TINY_KEEP)
            {
                int roomIndex = UnityEngine.Random.Range(0, rooms.Length);
                float x = (rooms[roomIndex].indexPosition.x - (gridSize.x / 2)) * tileSize;
                float y = (rooms[roomIndex].indexPosition.y - (gridSize.y / 2)) * tileSize;
                float z = (rooms[roomIndex].indexPosition.z - (gridSize.z / 2)) * tileSize;

                player.position = new Vector3(x, y, z);
            }
        }
    }
    
    void Init()
    {
        if(seed == string.Empty) seed = DateTime.Now.ToString();
        UnityEngine.Random.InitState(seed.GetHashCode());

        grid = new GridPoint[gridSize.x, gridSize.y, gridSize.z];
        verts = new List<Vector3>();
        uvs = new List<Vector2>();
        tris = new List<int>();
        buffer = 0;



        for (int x = 0;  x < gridSize.x; x++) 
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                for (int z = 0; z < gridSize.z; z++)
                {
                    grid[x, y, z] = new GridPoint();
                    grid[x, y, z].position = new Vector3(x - gridSize.x / 2, y - gridSize.y / 2, z - gridSize.z / 2) * tileSize;
                }
            }
        }
    }

    void PopulateValues(LevelGenerationAlgorithm algorithm)
    {
        if(algorithm == LevelGenerationAlgorithm.RANDOM_WALKER_2D)
        {
            RandomWalker(2);
        }
        else if(algorithm == LevelGenerationAlgorithm.RANDOM_WALKER_3D)
        {
            RandomWalker(3);
        }
        else if(algorithm == LevelGenerationAlgorithm.TINY_KEEP)
        {
            TinyKeep(numberOfRooms);
        }
    }

    void ActivateBox(Vector3Int cell, int maxX = 1, int maxY = 1, int maxZ = 1)
    {
        if (grid == null) return;
        if (maxX < 1 || maxY < 1 || maxZ < 1) return;

        for (int x = -maxX; x <= maxX; x++)
        {
            for (int y = -maxY; y <= maxY; y++)
            {
                for (int z = -maxZ; z <= maxZ; z++)
                {
                    if (cell.x + x >= gridSize.x - 1|| cell.x + x <= 0)
                    {
                        continue;
                    }
                    if (cell.y + y >= gridSize.y - 1 || cell.y + y <= 0)
                    {
                        continue;
                    }
                    if (cell.z + z >= gridSize.z - 1|| cell.z + z <= 0)
                    {
                        continue;
                    }

                    grid[cell.x + x, cell.y + y, cell.z + z].empty = true;

                }
            }
        }
    }

    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        //Use Marching Cubes To Generate a mesh 
        for (int x = 0; x < gridSize.x - 1; x++)
        {
            for (int y = 0; y < gridSize.y - 1; y++)
            {
                for (int z = 0; z < gridSize.z - 1; z++)
                {

                    GridPoint[] pointsInBox = new GridPoint[]
                    {
                            grid[x,y,z+1],
                            grid[x+1,y,z+1],
                            grid[x+1,y,z],
                            grid[x,y,z],
                            grid[x,y+1,z+1],
                            grid[x+1,y+1,z+1],
                            grid[x+1,y+1,z],
                            grid[x,y+1,z],
                    };

                    int cubeIndex = GridPoint.GetState(pointsInBox);


                    int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                    foreach (int edgeIndex in triangulation)
                    {
                        if (edgeIndex > -1)
                        {
                            int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                            int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                            
                            Vector3 vertexPos = GridPoint.GetMidPoint(pointsInBox[a], pointsInBox[b]);

                            verts.Add(vertexPos);
                            tris.Add(buffer);
                            buffer++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }


        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();

        for(int v = 0; v < verts.Count - 2; v+=3)
        {
            Vector2[] uvForTri = GetUVs(verts[v], verts[v + 1], verts[v + 2]);
            uvs.AddRange(uvForTri);
        }
        mesh.uv = uvs.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    
    void RandomWalker(int dimensions = 2)
    {
        if(dimensions < 2) dimensions = 2;
        if(dimensions > 3) dimensions = 3;

        Vector3Int currentIndex = gridSize / 2;

        for (int step = 0; step < numberOfSteps; step++)
        {
            if (dimensions == 2)
            {
                int x = UnityEngine.Random.Range(-1, 2);
                int z = UnityEngine.Random.Range(-1, 2);

                if (currentIndex.x + x > 0 && currentIndex.x + x < gridSize.x - 1)
                {
                    if (currentIndex.z + z > 0 && currentIndex.z + z < gridSize.z - 1)
                    {
                        currentIndex += new Vector3Int(x, 0, z);
                    }
                }
                ActivateBox(currentIndex);

            }
            else if (dimensions == 3) 
            {
                int x = UnityEngine.Random.Range(0, 2);
                int y = UnityEngine.Random.Range(0, 2);
                int z = UnityEngine.Random.Range(0, 2);


                if (currentIndex.x + x > 0 && currentIndex.x + x < gridSize.x - 1)
                {
                    if(currentIndex.y + y > 0 && currentIndex.y + y < gridSize.y - 1)
                    {
                        if (currentIndex.z + z > 0 && currentIndex.z + z < gridSize.z - 1)
                        {
                            currentIndex += new Vector3Int(x, y, z);
                            ActivateBox(currentIndex);
                        }
                    }
                }

            }
        }
    }

    void TinyKeep(int numberOfRooms = 2)
    {
        //Create Rooms
        rooms = new Room[numberOfRooms];
        for(int r = 0; r < numberOfRooms; r++) 
        {
            int xi = UnityEngine.Random.Range(0, gridSize.x);
            int yi = UnityEngine.Random.Range(0, gridSize.y);
            int zi = UnityEngine.Random.Range(0, gridSize.z);

            int roomSizeX = UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1);
            int roomSizeY = UnityEngine.Random.Range(1, maxRoomSize + 1);
            int roomSizeZ = UnityEngine.Random.Range(minRoomSize, maxRoomSize + 1);


            rooms[r] = new Room(new Vector3Int(xi, yi, zi), new Vector3Int(roomSizeX, roomHeight, roomSizeZ));
            ActivateBox(new Vector3Int(xi, yi, zi), roomSizeX, roomHeight, roomSizeZ);
        }

        //Create Hallways
        for (int r = 0; r < numberOfRooms; r++)
        {
            if (r + 1 < numberOfRooms)
            {
                Vector3Int currentPos = rooms[r].GetNearestExit(rooms[r + 1].indexPosition);
                Vector3Int end = rooms[r + 1].GetNearestExit(rooms[r].indexPosition);
                while (currentPos != end)
                {
                    Vector3Int[] possibleDirections = 
                    {
                        Vector3Int.left,
                        Vector3Int.right,
                        Vector3Int.up,
                        Vector3Int.down,
                        Vector3Int.forward,
                        Vector3Int.back,
                    };
                    Vector3Int chosenDirection = possibleDirections[0];
                    foreach (Vector3Int possibleDirection in possibleDirections)
                    {
                        if(Vector3Int.Distance(currentPos + chosenDirection, end) > Vector3Int.Distance(currentPos + possibleDirection, end))
                        {
                            chosenDirection = possibleDirection;
                        }
                    }

                    currentPos += chosenDirection;
                    ActivateBox(currentPos, hallwaySize, hallwaySize, hallwaySize);
                }
            }
        }
    }

    Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y);
            uvs[1] = new Vector2(b.z, b.y);
            uvs[2] = new Vector2(c.z, c.y);
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y);
            uvs[1] = new Vector2(b.x, b.y);
            uvs[2] = new Vector2(c.x, c.y);
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z);
            uvs[1] = new Vector2(b.x, b.z);
            uvs[2] = new Vector2(c.x, c.z);
        }

        return uvs;
    }
}
