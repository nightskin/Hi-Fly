using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent (typeof(MeshRenderer))]
[RequireComponent(typeof (MeshCollider))]
public class Asteroid : MonoBehaviour
{
    Voxel[] voxels = null;
    float isoLevel = 0;
    float OuterRadius;
    float innerRadius;

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
    
    public void RemoveBlock(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
        pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
        pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
        int i = ToVoxelIndex(pos);
        
        //If voxel is already deactivated check the next one
        if(voxels[i].value == -1)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * voxelSize / 2));
            pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
            pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
            pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize; 
            i = ToVoxelIndex(pos);
            voxels[i].value = -1;
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

    public void RemoveBlocksInRadius(RaycastHit hit, float radius)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        for (int i = 0; i < voxels.Length; i++)
        {
            if (Vector3.Distance(pos, voxels[i].position) <= radius)
            {
                voxels[i].value = -1;
            }
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
        OuterRadius = Random.Range(voxelSize * (voxelResolution - 1) / 4, voxelSize * (voxelResolution - 1) / 2);
        innerRadius = OuterRadius - Random.Range(0.0f, voxelSize);

        voxels = new Voxel[(int)Mathf.Pow(voxelResolution, 3)];
        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].position = ToPosition(i);
            float distanceFromCenter = Vector3.Distance(transform.position + (Vector3.one * OuterRadius), transform.position + voxels[i].position);
            if (distanceFromCenter > OuterRadius)
            {
                voxels[i].value = -1;
            }
            else
            {
                voxels[i].value = 1;
            }

        }
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

            Vector3[] triVerts = new Vector3[3];
            int triIndex = 0;

            foreach (int edgeIndex in triangulation)
            {
                if (edgeIndex > -1)
                {
                    int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                    int b = MarchingCubesTables.edgeConnections[edgeIndex][1];
                    Vector3 vertexPos = Voxel.MidPoint(points[a], points[b]);
                    verts.Add(vertexPos);
                    tris.Add(buffer);
                    
                    if(triIndex == 0)
                    {
                        triVerts[0] = vertexPos;
                        triIndex++;
                    }
                    else if(triIndex == 1)
                    {
                        triVerts[1] = vertexPos;
                        triIndex++;
                    }
                    else if(triIndex == 2)
                    {
                        triVerts[2] = vertexPos;
                        uvs.AddRange(GetUVs(triVerts[0], triVerts[1], triVerts[2]));
                        triIndex = 0;
                    }

                    buffer++;
                }
                else
                {
                    break;
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
            uvs[0] = new Vector2(a.z, a.y) / voxelSize;
            uvs[1] = new Vector2(b.z, b.y) / voxelSize;
            uvs[2] = new Vector2(c.z, c.y) / voxelSize;
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y) / voxelSize;
            uvs[1] = new Vector2(b.x, b.y) / voxelSize;
            uvs[2] = new Vector2(c.x, c.y) / voxelSize;
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z) / voxelSize;
            uvs[1] = new Vector2(b.x, b.z) / voxelSize;
            uvs[2] = new Vector2(c.x, c.z) / voxelSize;
        }

        return uvs;
    }

    bool BlocksGone()
    {
        for (int i = 0; i < voxelResolution * voxelResolution * voxelResolution; i++)
        {
            if (voxels[i].value > isoLevel) return false;
        }

        return true;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
