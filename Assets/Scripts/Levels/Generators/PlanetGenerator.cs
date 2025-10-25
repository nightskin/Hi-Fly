using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class PlanetGenerator : MonoBehaviour
{
    Noise noise;
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<int> triangles = new List<int>();
    int buffer = 0;
    

    [SerializeField] Gradient landGradient;
    [SerializeField][Min(1)] float gradientMult = 10;
    [SerializeField] int resolution = 50;

    [SerializeField] float radius = 100;
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
        Generate();
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
        noise = Galaxy.noise;

        GradientColorKey[] colorKeys = new GradientColorKey[landGradient.colorKeyCount]; 
        for (int i = 0; i < landGradient.colorKeyCount; i++)
        {
            if(i == 0)
            {
                colorKeys[i] = new GradientColorKey(Util.RandomColor(), 0);
            }
            else
            {
                colorKeys[i] = new GradientColorKey(Util.RandomColor(), 1 / i);
            }
        }
        landGradient.SetColorKeys(colorKeys);

        Color color1 = Util.RandomColor();
        Color color2 = Util.RandomColor();

        Transform water = transform.Find("Water");
        if (water)
        {
            water.localScale = Vector3.one * (radius + 10) * 2;
            water.GetComponent<MeshRenderer>().material.SetColor("_WaterColor", color1);
            water.GetComponent<MeshRenderer>().material.SetColor("_FoamColor", color2);
            water.gameObject.isStatic = true;
        }

        Transform rings = transform.Find("Rings");
        bool useRings = Util.RandomBool();
        if (useRings)
        {
            if (rings)
            {
                rings.localScale = Vector3.one * (radius + 100) * 2;
                rings.GetComponent<MeshRenderer>().material.SetColor("_Color1", color1);
                rings.GetComponent<MeshRenderer>().material.SetColor("_Color2", color2);
                rings.gameObject.isStatic = true;
            }
        }
        else
        {
            if(rings)
            {
                rings.gameObject.SetActive(false);
            }
        }


        vertices.Clear();
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
            colors.Add(landGradient.Evaluate(t));
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
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

    }
}
