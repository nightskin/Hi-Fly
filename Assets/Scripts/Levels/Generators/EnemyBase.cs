using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [SerializeField] float tileSize = 100;

    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int buffer = 0;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        



        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    void CreateTile(Vector3 position, Quaternion rotation)
    {
        verts.Add(rotation * (new Vector3(-0.5f, -0.5f, 0.5f) + position) * tileSize);
        verts.Add(rotation * (new Vector3(0.5f, -0.5f, 0.5f) + position) * tileSize);
        verts.Add(rotation * (new Vector3(0.5f, -0.5f, -0.5f) + position) * tileSize);
        verts.Add(rotation * (new Vector3(-0.5f, -0.5f, -0.5f) + position) * tileSize);

        tris.Add(0 + buffer);
        tris.Add(1 + buffer);
        tris.Add(2 + buffer);
        tris.Add(3 + buffer);
        tris.Add(0 + buffer);
        tris.Add(2 + buffer);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        buffer += 4;


    }

}
