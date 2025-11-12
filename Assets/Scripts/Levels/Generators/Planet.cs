using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Planet : MonoBehaviour
{
    [SerializeField] MeshRenderer planetMesh;
    [SerializeField] MeshRenderer cloudMesh;

    Voxel[] voxels = null;
    float radius;
    float isoLevel = 0.5f;

    [SerializeField] float cloudOffset = 10;
    public int voxelResolution = 10;
    public float voxelSize = 10;


    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    List<Color> colors = new List<Color>();
    int buffer = 0;

    int ToVoxelIndex(Vector3 position)
    {
        return ((int)(position.x / voxelSize)) + ((int)(position.y / voxelSize) * voxelResolution) + ((int)(position.z / voxelSize) * voxelResolution * voxelResolution);
    }

    Vector3 ToPosition(int i)
    {
        int x = i % voxelResolution;
        int y = i / voxelResolution % voxelResolution;
        int z = i / voxelResolution / voxelResolution % voxelResolution;
        return new Vector3(x, y, z) * voxelSize;
    }

    void Start()
    {
        Generate();
    }

    public void Generate()
    {

        verts.Clear(); 
        uvs.Clear(); 
        tris.Clear(); 
        buffer = 0;

        CreateVoxelData();
        CreateMeshData();

        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();

        planetMesh.material.SetColor("_LandColor", Util.RandomColor());
        planetMesh.material.SetColor("_WaterColor", Util.RandomColor());
        planetMesh.material.SetFloat("_NoiseScale", Random.Range(0.1f, 1f));
        planetMesh.material.SetVector("_Offset", Util.RandomVector4(-10,10));

        cloudMesh.transform.localScale = Vector3.one * (radius + cloudOffset) * 2;
        cloudMesh.transform.localPosition = Vector3.one * radius;
        cloudMesh.transform.gameObject.isStatic = true;
        cloudMesh.material.SetFloat("_Speed", Random.value);
        cloudMesh.material.SetVector("_Offset", Util.RandomVector4(-1, 1));
        cloudMesh.material.SetFloat("_NoiseScale", Random.Range(5, 10));

    }

    public void RemoveBlock(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
        pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
        pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
        int i = ToVoxelIndex(pos);

        //If voxel is already deactivated check the next one
        if (voxels[i].value < isoLevel)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * voxelSize / 2));
            pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
            pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
            pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
            i = ToVoxelIndex(pos);
            voxels[i].value = 0;
        }
        else
        {
            voxels[i].value = 0;
        }



        if (BlocksGone())
        {
            Destroy(gameObject);
        }
        else
        {
            colors.Clear();
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

        voxels = new Voxel[(int)Mathf.Pow(voxelResolution, 3)];
        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].position = ToPosition(i);
            
            float distanceFromCenter = Vector3.Distance(Vector3.one * radius, voxels[i].position);

            if (distanceFromCenter > radius)
            {
                voxels[i].color = Color.white;
                voxels[i].value = 0;
            }
            else
            {
                float maxDistance = Vector3.Distance(Vector3.one * radius, ToPosition(0));
                float invertedDistanceFromCenter = Mathf.Abs(distanceFromCenter - maxDistance);
                voxels[i].value = invertedDistanceFromCenter / maxDistance;

                if (voxels[i].value > 0.65f)
                {
                    voxels[i].color = Color.black;
                }
                else
                {
                    voxels[i].color = Color.white;
                }

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
                    Vector3 vertexPos = Voxel.LerpPoint(points[a], points[b], isoLevel);
                    verts.Add(vertexPos);
                    tris.Add(buffer);
                    colors.Add(Color.Lerp(points[a].color, points[b].color, isoLevel));

                    if (triIndex == 0)
                    {
                        triVerts[0] = vertexPos;
                        triIndex++;
                    }
                    else if (triIndex == 1)
                    {
                        triVerts[1] = vertexPos;
                        triIndex++;
                    }
                    else if (triIndex == 2)
                    {
                        triVerts[2] = vertexPos;
                        uvs.AddRange(Voxel.GetUVs(triVerts[0], triVerts[1], triVerts[2], voxelSize));
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
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
