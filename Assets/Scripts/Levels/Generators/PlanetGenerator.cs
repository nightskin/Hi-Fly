using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class PlanetGenerator : MonoBehaviour
{
    Voxel[] voxels = null;
    float radius;
    float isoLevel = 0.5f;

    [SerializeField] Color underGroundColor;
    [SerializeField] Color landColor;

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
        landColor = Util.RandomColor();

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

        transform.GetChild(0).localScale = Vector3.one * (radius + voxelSize) * 2;
        transform.GetChild(0).localPosition = Vector3.one * radius;
        transform.GetChild(0).gameObject.isStatic = true;
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_Tiling", Random.Range(1.0f, 10.0f));
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetFloat("_OffsetSpeed", Random.value);
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetVector("_Offset", new Vector4(Random.value, Random.value));
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
                voxels[i].color = landColor;
                voxels[i].value = 0;
            }
            else
            {
                float maxDistance = Vector3.Distance(Vector3.one * radius, ToPosition(0));
                float invertedDistanceFromCenter = Mathf.Abs(distanceFromCenter - maxDistance);
                voxels[i].value = invertedDistanceFromCenter / maxDistance;

                if (voxels[i].value > 0.65f)
                {
                    voxels[i].color = underGroundColor;
                }
                else
                {
                    voxels[i].color = landColor;
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
