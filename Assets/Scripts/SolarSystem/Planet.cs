using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Planet : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<int> triangles = new List<int>();
    int buffer = 0;

    public Color landColor;
    public Color waterColor;
    [SerializeField] int resolution = 200;

    public float radius;
    [SerializeField] float minRadius = 30;
    [SerializeField] float maxRadius = 50;
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
        radius = Random.Range(minRadius, maxRadius);
        landColor = Util.RandomColor();
        waterColor = Util.RandomColor();

        transform.GetChild(0).localScale = Vector3.one * (radius * 2 + (layers * 10));
        transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_WaterColor", waterColor);
        
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        buffer = 0;

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        Generate();

        UpdateMesh();
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

    void Generate()
    {
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
            colors.Add(landColor);
        }
    }


    float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int l = 0; l < 5; l++)
        {
            float v = Galaxy.noise.Evaluate(point * frequency + transform.position);
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
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
