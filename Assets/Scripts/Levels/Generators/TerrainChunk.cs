using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    [SerializeField] float minHeight = 0;
    [SerializeField] float maxHeight = 100;
    [SerializeField] bool useColors = false;

    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    Color[] colors;
    int[] triangles;

    void CreateTerrain()
    {
        vertices = new Vector3[(TerrainGenerator.chunkResolution + 1) * (TerrainGenerator.chunkResolution + 1)];
        uvs = new Vector2[vertices.Length];
        for (int i = 0, x = 0; x <= TerrainGenerator.chunkResolution; x++)
        {
            for (int z = 0; z <= TerrainGenerator.chunkResolution; z++)
            {
                float vx = (x - TerrainGenerator.chunkResolution / 2) * TerrainGenerator.voxelSize;
                float vz = (z - TerrainGenerator.chunkResolution / 2) * TerrainGenerator.voxelSize;
                float height = TerrainGenerator.noise.Evaluate((transform.position + new Vector3(vx, 0, vz)) * TerrainGenerator.noiseScale);
                height = Util.ConvertRange(-1, 1, minHeight, maxHeight, height);
                vertices[i] = new Vector3(vx, height, vz);
                uvs[i] = new Vector2(x, z) / TerrainGenerator.chunkResolution;
                i++;
            }
        }

        triangles = new int[TerrainGenerator.chunkResolution * TerrainGenerator.chunkResolution * 6];
        for (int v = 0, t = 0, x = 0; x < TerrainGenerator.chunkResolution; x++)
        {
            for (int z = 0; z < TerrainGenerator.chunkResolution; z++)
            {
                triangles[t] = v + TerrainGenerator.chunkResolution + 1;
                triangles[t + 1] = v;
                triangles[t + 2] = v + 1;
                
                triangles[t + 3] = v + TerrainGenerator.chunkResolution + 1;
                triangles[t + 4] = v + 1;
                triangles[t + 5] = v + TerrainGenerator.chunkResolution + 2;
                v++;
                t += 6;
            }
            v++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        if (useColors) mesh.colors = colors;
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void Generate()
    {
        mesh = new Mesh();
        CreateTerrain();
        UpdateMesh();
    }
}
