using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Buildings : MonoBehaviour
{
    [SerializeField][Min(1)] float buildingSpacing = 100;
    [SerializeField][Min(1)] float minBuildingHeight = 100;
    [SerializeField][Min(2)] float maxBuildingHeight = 500;
    [SerializeField][Min(2)] float maxBuildingSize = 75;
    [SerializeField][Min(1)] float minBuildingSize = 50;
    [SerializeField][Min(2)] int buildingsX = 10;
    [SerializeField][Min(2)] int buildingsZ = 10;
    [SerializeField] MeshCollider collider;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        
        // Make Meshes Here
        for(int x = 0; x < buildingsX; x++)
        {
            for(int z = 0; z < buildingsZ; z++)
            {
                int make = Mathf.RoundToInt(Random.value);
                if(make == 1)
                {
                    float height = Random.Range(minBuildingHeight, maxBuildingHeight);
                    float width = Random.Range(minBuildingSize, maxBuildingSize);
                    float depth = Random.Range(minBuildingSize, maxBuildingSize);
                    MakeBuilding(new Vector3(x, 0, z) * buildingSpacing, new Vector3(width, height, depth));
                }
            }
        }
        
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        if(!collider) collider = GetComponent<MeshCollider>();
        if(collider)
        {
            collider.sharedMesh = mesh;
        }
    }

    void MakeBuilding(Vector3 position, Vector3 size)
    {
        //Back Face
        verts.Add(new Vector3(0 * size.x, 1 * size.y, 0 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 1 * size.y, 0 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 0 * size.y, 0 * size.z) + position);
        verts.Add(new Vector3(0 * size.x, 0 * size.y, 0 * size.z) + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(0,1));
        uvs.Add(new Vector2(1,1));
        uvs.Add(new Vector2(1,0));
        uvs.Add(new Vector2(0,0));

        buffer += 4;

        //Front Face
        verts.Add(new Vector3(0 * size.x, 1 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 1 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 0 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(0 * size.x, 0 * size.y, 1 * size.z) + position);

        tris.Add(buffer + 3);
        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        buffer += 4;


        //Left Face
        verts.Add(new Vector3(0 * size.x, 0 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(0 * size.x, 1 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(0 * size.x, 1 * size.y, 0 * size.z) + position);
        verts.Add(new Vector3(0 * size.x, 0 * size.y, 0 * size.z) + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        buffer += 4;

        //Right Face
        verts.Add(new Vector3(1 * size.x, 0 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 1 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 1 * size.y, 0 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 0 * size.y, 0 * size.z) + position);

        tris.Add(buffer + 3);
        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);
        tris.Add(buffer + 1);
        tris.Add(buffer + 0);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        buffer += 4;

        //Top Face
        verts.Add(new Vector3(0 * size.x, 1 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 1 * size.y, 1 * size.z) + position);
        verts.Add(new Vector3(1 * size.x, 1 * size.y, 0 * size.z) + position);
        verts.Add(new Vector3(0 * size.x, 1 * size.y, 0 * size.z) + position);

        tris.Add(buffer + 0);
        tris.Add(buffer + 1);
        tris.Add(buffer + 2);
        tris.Add(buffer + 0);
        tris.Add(buffer + 2);
        tris.Add(buffer + 3);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        buffer += 4;
    }

}
