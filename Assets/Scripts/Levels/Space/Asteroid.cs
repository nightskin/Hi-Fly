using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
[RequireComponent(typeof (MeshCollider))]
public class Asteroid : MonoBehaviour
{
    Voxel[,,] voxels = null;

    [HideInInspector] public float radius;

    public int voxelsPerRow = 10;
    public float voxelSpacing = 10;
    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        CreateVoxelData();
        CreateMeshData();

        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

    }
    
    public async void RemoveBlock(Vector3 hit)
    {
        Vector3 targetPosition = transform.InverseTransformPoint(hit);
        Voxel closestVoxel = voxels[0,0,0];

        var result = await Task.Run(() => 
        {
            for (int x = 0; x < voxelsPerRow; x++)
            {
                for (int y = 0; y < voxelsPerRow; y++)
                {
                    for (int z = 0; z < voxelsPerRow; z++)
                    {
                        if (voxels[x, y, z].value > GameManager.isoLevel)
                        {
                            if (Vector3.Distance(targetPosition, closestVoxel.position) > Vector3.Distance(targetPosition, voxels[x, y, z].position))
                            {
                                closestVoxel = voxels[x, y, z];
                            }
                        }
                    }
                }
            }
            voxels[closestVoxel.index.x, closestVoxel.index.y, closestVoxel.index.z].value = -1;

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
                CreateMeshData();
                UpdateMesh();
            }
        }
    }
    
    void CreateVoxelData()
    {
        voxels = new Voxel[voxelsPerRow + 1, voxelsPerRow + 1, voxelsPerRow + 1];
        radius = Random.Range(voxelsPerRow * voxelSpacing / 4, voxelsPerRow * voxelSpacing / 2);
        for (int x = 0; x < voxelsPerRow + 1; x++)
        {
            for (int y = 0; y < voxelsPerRow + 1; y++)
            {
                for (int z = 0; z < voxelsPerRow + 1; z++)
                {
                    voxels[x, y, z] = new Voxel();
                    voxels[x, y, z].index = new Vector3Int(x, y, z);
                    voxels[x, y, z].position = new Vector3(x, y, z) * voxelSpacing;

                    float distanceFromCenter = Vector3.Distance(transform.position + (Vector3.one * radius), transform.position + voxels[x, y, z].position);
                    if (distanceFromCenter > radius)
                    {
                        voxels[x, y, z].value = -1;
                    }
                    else
                    {
                        voxels[x, y, z].value = 1;
                    }
                }
            }
        }
    }
    
    void CreateMeshData()
    {
        for (int x = voxelsPerRow; x > 0; x--)
        {
            for (int y = voxelsPerRow; y > 0; y--)
            {
                for (int z = voxelsPerRow; z > 0; z--)
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
        for (int x = 0; x < voxelsPerRow; x++)
        {
            for (int y = 0; y < voxelsPerRow; y++)
            {
                for (int z = 0; z < voxelsPerRow; z++)
                {
                    if (voxels[x, y, z].value > GameManager.isoLevel) return false;
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
