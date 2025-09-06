using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class DestructibleTerrainChunk : MonoBehaviour
{
    [Range(0,1) ]public float isoLevel = 0.5f;

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
        Vector3 pos = transform.InverseTransformPoint(hit.point);

        pos.x = Mathf.Round(pos.x / TerrainGenerator.voxelSize) * TerrainGenerator.voxelSize;
        pos.y = Mathf.Round(pos.y / TerrainGenerator.voxelSize) * TerrainGenerator.voxelSize;
        pos.z = Mathf.Round(pos.z / TerrainGenerator.voxelSize) * TerrainGenerator.voxelSize;
        int i = Voxel.PositionToIndex(pos,TerrainGenerator.chunkResolution,TerrainGenerator.voxelSize);

        if (voxels[i].position.y == 0) return;
        
        //If voxel is already deactivated check the next one
        if (voxels[i].value <= 0)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * TerrainGenerator.voxelSize / 2));
            pos.x = Mathf.Round(pos.x / TerrainGenerator.voxelSize) * TerrainGenerator.voxelSize;
            pos.y = Mathf.Round(pos.y / TerrainGenerator.voxelSize) * TerrainGenerator.voxelSize;
            pos.z = Mathf.Round(pos.z / TerrainGenerator.voxelSize) * TerrainGenerator.voxelSize;
            i = Voxel.PositionToIndex(pos, TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize);
            voxels[i].value -= damage;
        }
        else
        {
            voxels[i].value -= damage;
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

        CreateVoxelData();
        CreateMeshData();
        UpdateMesh();
    }

    void CreateVoxelData()
    {
        voxels = new Voxel[TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution];
        for (int i = 0; i < TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].position = Voxel.IndexToPosition(i, TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize);

            int yi = Mathf.RoundToInt(voxels[i].position.y / TerrainGenerator.chunkResolution);

            if (voxels[i].position.y == 0)
            {
                voxels[i].value = 1;
            }
            else
            {
                float px = (transform.position.x + voxels[i].position.x) * TerrainGenerator.noiseScale;
                float pz = (transform.position.z + voxels[i].position.z) * TerrainGenerator.noiseScale;
                voxels[i].value = Util.ConvertRange(-1, 1, 0, 1, TerrainGenerator.noise.Evaluate(new Vector3(px, 0, pz))) - Util.ConvertRange(0, TerrainGenerator.chunkResolution, 0, 1, yi);
            }

            if (voxels[i].position.x == 0 || voxels[i].position.z == 0 || voxels[i].position.x == (TerrainGenerator.chunkResolution - 1) * TerrainGenerator.voxelSize || voxels[i].position.z == (TerrainGenerator.chunkResolution - 1) * TerrainGenerator.voxelSize)
            {
                voxels[i].value = 0;
            }
        }
    }

    void CreateMeshData()
    {
        mesh = new Mesh();
        mesh.MarkDynamic();
        for (int i = TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution; i > 0; i--)
        {
            Vector3 position = Voxel.IndexToPosition(i, TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize);
            Voxel[] points = new Voxel[]
            {
                voxels[Voxel.PositionToIndex(position + new Vector3(0,0,-1), TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position +  new Vector3(-1, 0, -1), TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position +  new Vector3(-1, 0, 0), TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position, TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(0, -1, -1), TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(-1,-1,-1),TerrainGenerator.chunkResolution ,TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(-1,-1, 0), TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)],
                voxels[Voxel.PositionToIndex(position + new Vector3(0, -1, 0), TerrainGenerator.chunkResolution, TerrainGenerator.voxelSize)]
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
                        uvs.AddRange(Voxel.GetUVs(triVerts[0], triVerts[1], triVerts[2], TerrainGenerator.voxelSize));
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
