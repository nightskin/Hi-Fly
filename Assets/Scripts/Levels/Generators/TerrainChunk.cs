using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainChunk : MonoBehaviour
{
    public int voxelResolution = 10;
    public float voxelSize = 10;
    [Range(0,1) ]public float isoLevel = 0.5f;
    public float noiseScale = 0.01f;

    Noise noise;

    [SerializeField] Mesh mesh;
    [SerializeField] Voxel[] voxels;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] List<Color> colors = new List<Color>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] int buffer = 0;


    void Start()
    {
        if (vertices.Count == 0)
        {
            Generate();
        }
        else
        {
            UpdateMesh();
        }
    }

    public void TeraForm(RaycastHit hit, float damage)
    {
        Vector3 hitPoint = transform.InverseTransformPoint(hit.point);
        int nearestIndex = 0;

        for (int i = 0; i < voxelResolution * voxelResolution * voxelResolution; i++)
        {
            float iDist = Vector3.Distance(hitPoint, voxels[i].position);
            float nearestDist = Vector3.Distance(hitPoint, voxels[nearestIndex].position);
            if (iDist < nearestDist)
            {
                nearestIndex = i;
            }
        }

        if (voxels[nearestIndex].position.y > 0)
        {
            voxels[nearestIndex].value -= damage;
        }


        vertices.Clear();
        uvs.Clear();
        colors.Clear();
        triangles.Clear();
        buffer = 0;

        CreateMeshData();
        UpdateMesh();
    }

    public void Generate()
    {
        vertices.Clear();
        colors.Clear();
        uvs.Clear();
        triangles.Clear();

        noise = new Noise(TerrainGenerator.seed.GetHashCode());

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateVoxelData();
        CreateMeshData();

        UpdateMesh();
    }

    void CreateVoxelData()
    {
        voxels = new Voxel[voxelResolution * voxelResolution * voxelResolution];
        for (int i = 0; i < voxelResolution * voxelResolution * voxelResolution; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].position = Voxel.IndexToPosition(i, voxelResolution, voxelSize);
            int y = Mathf.RoundToInt(voxels[i].position.y / voxelResolution);
            if (voxels[i].position.y == 0)
            {
                voxels[i].value = 1;
            }
            else
            {
                float px = (transform.position.x + voxels[i].position.x) * noiseScale;
                float pz = (transform.position.z + voxels[i].position.z) * noiseScale;
                voxels[i].value = Util.ConvertRange(-1, 1, 0, 1, noise.Evaluate(new Vector3(px, 0, pz))) - Util.ConvertRange(0, voxelResolution, 0, 1, y);
            }
        }
    }

    void CreateMeshData()
    {
        for (int i = voxels.Length; i > 0; i--)
        {
            Vector3 position = Voxel.IndexToPosition(i, voxelResolution, voxelSize);
            Voxel[] points = new Voxel[]
            {
                voxels[Voxel.PositionToIndex(position + new Vector3(0,0,-1), voxelResolution, voxelSize)],
                voxels[Voxel.PositionToIndex(position +  new Vector3(-1, 0, -1), voxelResolution, voxelSize)],
                voxels[Voxel.PositionToIndex(position +  new Vector3(-1, 0, 0), voxelResolution, voxelSize)],
                voxels[Voxel.PositionToIndex(position, voxelResolution, voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(0, -1, -1), voxelResolution, voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(-1,-1,-1),voxelResolution ,voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(-1,-1, 0), voxelResolution, voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(0, -1, 0), voxelResolution, voxelSize)]
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
                    vertices.Add(vertexPos);
                    triangles.Add(buffer);
                    
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

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
