using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TerrainGenerator : MonoBehaviour
{
    public enum TerrainType
    {
        MOUNTAINS,
        ISLANDS,
    }
    public TerrainType type;

    public string seed = string.Empty;
    public int resolution = 100;
    public float spacing = 10;

    [Range(0,1)] public float persistance = 0.5f;
    [Min(1.01f)] public float lacunarity = 2;

    [Min(1)] public float maxHeight = 10;
    [Min(1)] public float minHeight = 10;
    public float noiseScale = 0.01f;
    public Vector3 noiseOffset = Vector3.zero;


    Noise noise;
    [SerializeField] Mesh mesh;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] int buffer = 0;

    void Start()
    {
        if(vertices.Count == 0)
        {
            Generate();
        }
        else
        {
            UpdateMesh();
        }
        
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void Generate()
    {
        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        buffer = 0;

        noise = new Noise(seed.GetHashCode());
        
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        // Create Ground
        CreateVertexGrid(Quaternion.Euler(0, 0, 180));
        for(int i = 0; i < vertices.Count; i++)
        {
            float y = Evaluate(vertices[i] * noiseScale + noiseOffset);
            
            if(type == TerrainType.MOUNTAINS)
            {
                if(y > 0)
                {
                    vertices[i] = new Vector3(vertices[i].x, y * maxHeight, vertices[i].z);
                }
                else
                {
                    vertices[i] = new Vector3(vertices[i].x, y * minHeight, vertices[i].z);
                }
            }
            else if(type == TerrainType.ISLANDS)
            {
                if(y > 0)
                {
                    vertices[i] = new Vector3(vertices[i].x, maxHeight, vertices[i].z);
                }
                else
                {
                    vertices[i] = new Vector3(vertices[i].x, y * minHeight, vertices[i].z);
                }
            }
        }
        UpdateMesh();
    }

    void CreateVertexGrid(Quaternion rotation)
    {
        for (int x = 0; x <= resolution; x++)
        {
            for (int z = 0; z <= resolution; z++)
            {
                vertices.Add(rotation * new Vector3(x - resolution / 2, 0, z - resolution / 2) * spacing);
                uvs.Add(new Vector2(x,z) / resolution);
            }
        }

        for (int v = 0, t = 0, x = 0; x < resolution; x++)
        {
            for (int z = 0; z < resolution; z++)
            {
                triangles.Insert(t + 0, v + 0 + buffer);
                triangles.Insert(t + 1, v + resolution + 1 + buffer);
                triangles.Insert(t + 2, v + 1 + buffer);
                triangles.Insert(t + 3, v + 1 + buffer);
                triangles.Insert(t + 4, v + resolution + 1 + buffer);
                triangles.Insert(t + 5, v + resolution + 2 + buffer);

                v++;
                t += 6;
            }
            v++;
        }

        buffer += (resolution + 1) * (resolution + 1);
    }

    float Evaluate(Vector3 point, int layers = 3)
    {
        float amplitude = 1;
        float frequency = 1;
        float height = 0;
        for(int l = 0; l < layers; l++)
        {
            height += noise.Evaluate(point * frequency) * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }
        return height;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}
