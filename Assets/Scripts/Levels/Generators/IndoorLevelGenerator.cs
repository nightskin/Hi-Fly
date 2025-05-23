using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class IndoorLevelGenerator : MonoBehaviour
{
    //public static int seed = 0;
    Voxel[] voxels = null;

    [SerializeField] int voxelResolution = 100;
    [SerializeField]float voxelSize = 10;
    float isoLevel = 0;


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
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();
    }

    void CreateVoxelData()
    {
        voxels = new Voxel[(int)Mathf.Pow(voxelResolution, 3)];
        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].index = i;
            voxels[i].position = ToPosition(i);
            

        }
    }

    int ToVoxelIndex(Vector3 position)
    {
        return ((int)(position.x / voxelSize)) + ((int)(position.y / voxelSize) * voxelResolution) + ((int)(position.z / voxelSize) * voxelResolution * voxelResolution);
    }

    void CreateMeshData()
    {
        for (int i = voxels.Length; i > 0; i--)
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


            int cubeIndex = Voxel.GetState(points, isoLevel);
            int[] triangulation = MarchingCubesTables.triTable[cubeIndex];
            foreach (int edgeIndex in triangulation)
            {
                if (edgeIndex > -1)
                {
                    int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                    int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                    Vector3 vertexPos = Voxel.MidPoint(points[a], points[b]);
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

    Vector3 ToPosition(int i)
    {
        int x = i % voxelResolution;
        int y = i / voxelResolution % voxelResolution;
        int z = i / voxelResolution / voxelResolution % voxelResolution;
        return new Vector3(x, y, z) * voxelSize;
    }

}
