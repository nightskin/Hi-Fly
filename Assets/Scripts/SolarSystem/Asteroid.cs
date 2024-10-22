using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
[RequireComponent(typeof (MeshCollider))]
public class Asteroid : MonoBehaviour
{
    Voxel[,,] voxels = null;

    public float radius;
    public int tiles = 10;
    public float tileSize = 10;
    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        radius = Random.Range(tiles * tileSize / 4, tiles * tileSize / 2);
        CreateVoxelData();
        MarchingCubes();

        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

    }
    
    public async void RemoveBlock(RaycastHit hit)
    {
        Vector3 targetPosition = hit.point - transform.position;
        Voxel closestVoxel = voxels[0,0,0];

        var result = await Task.Run(() => 
        {
            for (int x = 0; x < tiles; x++)
            {
                for (int y = 0; y < tiles; y++)
                {
                    for (int z = 0; z < tiles; z++)
                    {
                        if (voxels[x, y, z].active)
                        {
                            if (Vector3.Distance(targetPosition, closestVoxel.position) > Vector3.Distance(targetPosition, voxels[x, y, z].position))
                            {
                                closestVoxel = voxels[x, y, z];
                            }
                        }
                    }
                }
            }

            voxels[closestVoxel.x, closestVoxel.y, closestVoxel.z].active = false;

            return true;
        });

        if(result)
        {
            if (BlocksGone())
            {
                Destroy(gameObject);
            }
            else
            {

                verts.Clear();
                tris.Clear();
                uvs.Clear();
                buffer = 0;
                MarchingCubes();
                UpdateMesh();
            }
        }
    }

    void CreateVoxelData()
    {
        voxels = new Voxel[tiles + 1, tiles + 1, tiles + 1];

        Plane[] planes =
        {
            new Plane(new Vector3(-1, Random.Range(-1,1), Random.Range(-1,1)), new Vector3(radius - tileSize, 0, 0)),
            new Plane(new Vector3(1, Random.Range(-1,1), Random.Range(-1,1)), new Vector3(-radius + tileSize, 0, 0)),

            new Plane(new Vector3(Random.Range(-1,1), Random.Range(-1,1), 1), new Vector3(0, 0, -radius + tileSize)),
            new Plane(new Vector3(Random.Range(-1,1), Random.Range(-1,1), -1), new Vector3(0, 0, radius - tileSize)),
        };


        for (int x = 0; x < tiles + 1; x++)
        {
            for (int y = 0; y < tiles + 1; y++)
            {
                for (int z = 0; z < tiles + 1; z++)
                {
                    voxels[x, y, z] = new Voxel();
                    voxels[x, y, z].x = x;
                    voxels[x, y, z].y = y;
                    voxels[x, y, z].z = z;
        
                    voxels[x, y, z].position = new Vector3(x - tiles / 2, y - tiles / 2, z - tiles / 2) * tileSize;
                    float distanceFromCenter = Vector3.Distance(transform.position, transform.position + voxels[x,y,z].position);
                    
                    if(distanceFromCenter > radius)
                    {
                        voxels[x, y, z].active = false;
                    }
                    else
                    {
                        voxels[x, y, z].active = EvaluatePoint(voxels[x, y, z].position, planes);
                    }
                }
            }
        }
    }
    
    bool EvaluatePoint(Vector3 point, Plane[] planes)
    {
        foreach (Plane plane in planes)
        {
            if (plane.GetDistanceToPoint(point) <= 0)
            {
                return false;
            }
        }
        return true;
    }

    void MarchingCubes()
    {
        for(int x = tiles; x > 0; x--)
        {
            for(int y = tiles; y > 0; y--)
            {
                for (int z = tiles; z > 0; z--)
                {
                    Voxel[] points = new Voxel[]
                        {
                            voxels[x,y,z-1],
                            voxels[x-1,y,z-1],
                            voxels[x-1,y,z],
                            voxels[x,y,z],
                            voxels[x,y-1,z-1],
                            voxels[x-1,y-1,z-1],
                            voxels[x-1,y-1,z],
                            voxels[x,y-1,z],
                        };

                    int cubeIndex = Voxel.GetState(points);


                    int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
                    foreach (int edgeIndex in triangulation)
                    {
                        if (edgeIndex > -1)
                        {
                            int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                            int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                            
                            Vector3 vertexPos = Voxel.GetMidPoint(points[a], points[b]);




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

    bool BlocksGone()
    {
        for (int x = 0; x < tiles; x++)
        {
            for (int y = 0; y < tiles; y++)
            {
                for (int z = 0; z < tiles; z++)
                {
                    if (voxels[x, y, z].active) return false;
                }
            }
        }
        return true;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        for (int v = 0; v < verts.Count - 2; v += 3)
        {
            Vector2[] uvForTri = GetUVs(verts[v], verts[v + 1], verts[v + 2]);
            uvs.AddRange(uvForTri);
        }
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
