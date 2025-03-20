using System.Collections.Generic;
using UnityEngine;

public class CityIsland : MonoBehaviour
{
    [SerializeField] float pointSpacing = 10;
    [SerializeField] int points = 100;
    [SerializeField] float noiseScale = 0.001f;

    Mesh mesh;
    Vector3[] verts;
    int[] tris;

    void Start()
    {
        GameManager.InitRandom();
        
        CreateMeshData();

        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();
    }
    
    void CreateMeshData()
    {
        verts = new Vector3[(points + 1) * (points + 1)];
        for(int i = 0, z = 0; z <= points; z++) 
        {
            for (int x = 0; x <= points; x++)
            {
                float h = GameManager.noise.Evaluate(new Vector3(x, 0, z) * noiseScale);
                verts[i] = new Vector3(x - points / 2, h, z - points / 2) * pointSpacing;
                i++;
            }
        }

        tris = new int[points * points * 6];
        
        int v = 0;
        int t = 0;
        for (int z = 0; z < points; z++)
        {
            for(int x = 0; x < points; x++)
            {
                tris[t + 0] = v + 0;
                tris[t + 1] = v + points + 1;
                tris[t + 2] = v + 1;
                tris[t + 3] = v + 1;
                tris[t + 4] = v + points + 1;
                tris[t + 5] = v + points + 2;

                v++;
                t += 6;
            }
            v++;
        }
    }
    
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        //mesh.uv = uvs.ToArray();

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
