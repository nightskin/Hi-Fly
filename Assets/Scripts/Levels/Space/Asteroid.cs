using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
[RequireComponent(typeof (MeshCollider))]
public class Asteroid : MonoBehaviour
{
    Voxel[,,] voxels3D = null;
    Voxel[] voxels = null;

    Vector3 hit_Debug = -Vector3.one;

    [HideInInspector] public float radius;
    public bool useOneDArray = false;
    public int voxelResolution = 10;
    public float voxelSize = 10;
    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    int ToVoxelIndex(Vector3 position)
    {
        return ((int)(position.x / voxelSize)) + ((int)(position.y / voxelSize)  * voxelResolution) + ((int)(position.z / voxelSize) * voxelResolution * voxelResolution);
    }

    Vector3 ToPosition(int i)
    {
        int x  = i % voxelResolution;
        int y = i / voxelResolution % voxelResolution;
        int z = i / voxelResolution / voxelResolution % voxelResolution;
        return new Vector3(x,y,z) * voxelSize;
    }


    void Start()
    {
        CreateVoxelData();
        CreateMeshData();

        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

    }

    void OnDrawGizmos()
    {
        if(hit_Debug != -Vector3.one)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.TransformPoint(hit_Debug), Vector3.one * voxelSize);
        }
    }

    public async void RemoveBlock(Vector3 hit)
    {
        Vector3 targetPosition = transform.InverseTransformPoint(hit);
        Voxel closestVoxel = voxels3D[0,0,0];

        var result = await Task.Run(() => 
        {
            for (int x = 0; x < voxelResolution; x++)
            {
                for (int y = 0; y < voxelResolution; y++)
                {
                    for (int z = 0; z < voxelResolution; z++)
                    {
                        if (voxels3D[x, y, z].value > GameManager.isoLevel)
                        {
                            if (Vector3.Distance(targetPosition, closestVoxel.position) > Vector3.Distance(targetPosition, voxels3D[x, y, z].position))
                            {
                                closestVoxel = voxels3D[x, y, z];
                            }
                        }
                    }
                }
            }
            voxels3D[closestVoxel.index3D.x, closestVoxel.index3D.y, closestVoxel.index3D.z].value = -1;

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
    
    public void RemoveBlock(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
        pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
        pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
        int i = ToVoxelIndex(pos);

        if(voxels[i].value == -1)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * voxelSize / 2));
            pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
            pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
            pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize; 
            i = ToVoxelIndex(pos);
            if(voxels[i].value == -1)
            {
                hit_Debug = ToPosition(i);
            }
            else
            {
                voxels[i].value = -1;
            }
        }
        else
        {
            voxels[i].value = -1;
        }

        
        
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

    void CreateVoxelData()
    {
        radius = Random.Range(voxelSize * (voxelResolution - 1) / 4, voxelSize * (voxelResolution - 1) / 2);

        if(useOneDArray)
        {
            voxels = new Voxel[(int)Mathf.Pow(voxelResolution, 3)];
            for(int i = 0; i < voxels.Length; i++)
            {
                voxels[i] = new Voxel();
                voxels[i].index = i;
                voxels[i].position = ToPosition(i);
                float distanceFromCenter = Vector3.Distance(transform.position + (Vector3.one * radius), transform.position + voxels[i].position);
                if (distanceFromCenter > radius)
                {
                    voxels[i].value = -1;
                }
                else
                {
                    voxels[i].value = 1;
                }

            }
        }
        else
        {
            voxels3D = new Voxel[voxelResolution, voxelResolution, voxelResolution];
            for (int x = 0; x < voxelResolution; x++)
            {
                for (int y = 0; y < voxelResolution; y++)
                {
                    for (int z = 0; z < voxelResolution; z++)
                    {
                        voxels3D[x, y, z] = new Voxel();
                        voxels3D[x, y, z].index3D = new Vector3Int(x, y, z);
                        voxels3D[x, y, z].position = new Vector3(x, y, z) * voxelSize;

                        float distanceFromCenter = Vector3.Distance(transform.position + (Vector3.one * radius), transform.position + voxels3D[x, y, z].position);
                        if (distanceFromCenter > radius)
                        {
                            voxels3D[x, y, z].value = -1;
                        }
                        else
                        {
                            voxels3D[x, y, z].value = 1;
                        }
                    }
                }
            }
        }
    }
    
    void CreateMeshData()
    {
        if(useOneDArray)
        {
            for(int i = voxels.Length; i > 0; i--)
            {
                Vector3 position = ToPosition(i);

                Voxel[] points = new Voxel[]
                {
                    voxels[ToVoxelIndex(position + new Vector3(0,0,-1))],
                    voxels[ToVoxelIndex(position +  new Vector3(-1, 0, -1))],
                    voxels[ToVoxelIndex(position +  new Vector3(-1, 0, 0))],
                    voxels[ToVoxelIndex(position)],
                    voxels[ToVoxelIndex(position + new Vector3(0, -1, -1))],
                    voxels[ToVoxelIndex(position + new Vector3(-1,-1,-1))],
                    voxels[ToVoxelIndex(position + new Vector3(-1,-1, 0))],
                    voxels[ToVoxelIndex(position + new Vector3(0, -1, 0))]
                };


                int cubeIndex = Voxel.GetStateCube(points);
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
        else
        {
            for (int x = voxelResolution - 1; x > 0; x--)
            {
                for (int y = voxelResolution - 1; y > 0; y--)
                {
                    for (int z = voxelResolution - 1; z > 0; z--)
                    {
                        Voxel[] points = new Voxel[]
                        {
                        voxels3D[x,y,z-1],
                        voxels3D[x-1,y,z-1],
                        voxels3D[x-1,y,z],
                        voxels3D[x,y,z],
                        voxels3D[x,y-1,z-1],
                        voxels3D[x-1,y-1,z-1],
                        voxels3D[x-1,y-1,z],
                        voxels3D[x,y-1,z],
                        };

                        int cubeIndex = Voxel.GetStateCube(points);


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
        if(useOneDArray)
        {
            for(int i  = 0; i < voxelResolution * voxelResolution * voxelResolution; i++)
            {
                if (voxels[i].value > GameManager.isoLevel) return false;
            }
        }
        else
        {
            for (int x = 0; x < voxelResolution; x++)
            {
                for (int y = 0; y < voxelResolution; y++)
                {
                    for (int z = 0; z < voxelResolution; z++)
                    {
                        if (voxels3D[x, y, z].value > GameManager.isoLevel) return false;
                    }
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
