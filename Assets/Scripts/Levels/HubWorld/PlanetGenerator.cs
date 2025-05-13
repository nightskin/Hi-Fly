using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class PlanetGenerator : MonoBehaviour
{
    public string seed = "";
    [SerializeField] Noise noise;
    [SerializeField] Mesh mesh;
    [SerializeField] List<Vector3> vertices = new List<Vector3>();
    [SerializeField] List<Vector2> uvs = new List<Vector2>();
    [SerializeField] List<Color> colors = new List<Color>();
    [SerializeField] List<int> triangles = new List<int>();
    [SerializeField] int buffer = 0;

    [SerializeField] bool generateColors = true;
    [SerializeField] Gradient landGradient;
    [SerializeField][Min(1)] float gradientMult = 10;
    [SerializeField] Color waterColor;
    [SerializeField] int resolution = 50;

    [SerializeField] float radius = 100;
    [SerializeField] float waterLevel = 100;
    [SerializeField] float baseRoughness = 1;
    [SerializeField] float roughness = 2;
    [SerializeField] float strength = 1;
    [SerializeField] float persistance = 0.5f;
    [SerializeField] float minValue = 1;
    [SerializeField][Range(1, 10)] int layers = 1;

    
    void CreateVertexGrid(Quaternion rotation ,float size)
    {
        for (int x = 0; x <= resolution; x++)
        {
            for (int z = 0; z <= resolution; z++)
            {
                vertices.Add(rotation * new Vector3(x - resolution / 2, -resolution / 2, z - resolution / 2) * size);
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


    void Start()
    {
        if (vertices.Count == 0) 
        {
            Generate();
        }
        else
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
            UpdateMesh();
        }
        
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    void DrawCube()
    {
        //BottomFace
        CreateVertexGrid(Quaternion.Euler(0, 0, 0), 1);
        //TopFace
        CreateVertexGrid(Quaternion.Euler(180, 0, 0), 1);

        //FrontFace
        CreateVertexGrid(Quaternion.Euler(-90, 0, 0), 1);
        //BackFace
        CreateVertexGrid(Quaternion.Euler(90, 0, 0), 1);

        //RightFace
        CreateVertexGrid(Quaternion.Euler(0, 0, -90), 1);
        //LeftFace
        CreateVertexGrid(Quaternion.Euler(0, 0, 90), 1);

    }

    public void Generate()
    {
        if(seed == string.Empty)
        {
            noise = transform.root.GetComponent<Galaxy>().noise;
        }
        else
        {
            noise = new Noise(seed.GetHashCode());
        }


        Transform water = transform.Find("Water");
        if (water)
        {
            water.localScale = Vector3.one * waterLevel * 2;
            water.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_WaterColor", waterColor);
        }

        vertices.Clear();
        uvs.Clear();
        triangles.Clear();
        colors.Clear();
        buffer = 0;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        DrawCube();
        for (int v = 0; v < vertices.Count; v++)
        {
            float firstlayerValue = 0;
            float elevation = 0;

            vertices[v] = vertices[v].normalized;

            if(layers > 0)
            {
                firstlayerValue = Evaluate(vertices[v]);
                elevation = firstlayerValue;
            }
            for(int l = 1; l < layers; l++)
            {
                float mask = (firstlayerValue > 0) ? firstlayerValue : 1;
                elevation += Evaluate(vertices[v]) * mask;
            }
            vertices[v] *= radius * (1 + elevation);
            float t = elevation * gradientMult;
            if(generateColors) colors.Add(landGradient.Evaluate(t));
        }
        UpdateMesh();
    }
    
    float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int l = 0; l < 5; l++)
        {
            float v = noise.Evaluate(point * frequency + transform.position);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        noiseValue = Mathf.Max(0, noiseValue - minValue);
        return noiseValue * strength;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        if(generateColors)
        {
            mesh.colors = colors.ToArray();
        }
        else
        {
            mesh.uv = uvs.ToArray();
        }
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

    }
}
