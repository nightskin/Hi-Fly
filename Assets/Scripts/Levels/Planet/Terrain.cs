using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Terrain : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    int buffer = 0;
    Vector2Int resolution = Vector2Int.one;
    
    
    [SerializeField][Min(1)] float maxHeight = 100;
    [SerializeField][Min(1)] float vertexSpacing = 1;
    [SerializeField] Texture2D heightMap;

    void CreateTerrainMesh(Quaternion rotation ,float vertexSpacing)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        for (int x = 0; x <= resolution.x; x++)
        {
            for (int y = 0; y <= resolution.y; y++)
            {
                float height =  heightMap.GetPixel(x,y).grayscale * maxHeight;
                vertices.Add(rotation * new Vector3(x - resolution.x / 2, height, y - resolution.y / 2) * vertexSpacing);
                uvs.Add(new Vector2(x, y));
            }
        }

        for (int v = 0, t = 0, x = 0; x < resolution.x; x++)
        {
            for (int y = 0; y < resolution.y; y++)
            {
                triangles.Insert(t + 0, v + 0 + buffer);
                triangles.Insert(t + 1, v + resolution.x + 1 + buffer);
                triangles.Insert(t + 2, v + 1 + buffer);
                triangles.Insert(t + 3, v + 1 + buffer);
                triangles.Insert(t + 4, v + resolution.x + 1 + buffer);
                triangles.Insert(t + 5, v + resolution.x + 2 + buffer);

                v++;
                t += 6;
            }
            v++;
        }

        buffer += (resolution.x + 1) * (resolution.y + 1);
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
    void Start()
    {
        if(heightMap)
        {
            resolution = new Vector2Int(heightMap.width, heightMap.height);
            CreateTerrainMesh(Quaternion.Euler(0,0,180), vertexSpacing);
            UpdateMesh();
            if(GetComponent<MeshCollider>())
            {
                GetComponent<MeshCollider>().sharedMesh = mesh;
            }

        }

    }

}
