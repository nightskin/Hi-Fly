using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class PlanetChunk : MonoBehaviour
{
    Planet planet;
    Voxel[] voxels = null;
    
    Mesh mesh;
    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Color> colors = new List<Color>();
    int buffer = 0;

    public void Generate()
    {
        planet = transform.parent.GetComponent<Planet>();

        colors.Clear();
        verts.Clear(); 
        tris.Clear(); 
        buffer = 0;

        CreateVoxelData();
        CreateMeshData();

        mesh = new Mesh();
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        UpdateMesh();


        GetComponent<MeshRenderer>().material.SetVector("_Offset", new Vector4(transform.position.x, transform.position.y, transform.position.z, 0));
        GetComponent<MeshRenderer>().material.SetColor("_LandColor", planet.landColor);
        GetComponent<MeshRenderer>().material.SetColor("_WaterColor", planet.WaterColor);
    }

    public void RemoveBlock(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / planet.voxelSize) * planet.voxelSize;
        pos.y = Mathf.Round(pos.y / planet.voxelSize) * planet.voxelSize;
        pos.z = Mathf.Round(pos.z / planet.voxelSize) * planet.voxelSize;
        int i = Voxel.PositionToIndex(pos, planet.voxelsPerChunk, planet.voxelSize);

        //If voxel is already deactivated check the next one
        if (voxels[i].value < planet.isoLevel)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * planet.voxelSize / 2));
            pos.x = Mathf.Round(pos.x / planet.voxelSize) * planet.voxelSize;
            pos.y = Mathf.Round(pos.y / planet.voxelSize) * planet.voxelSize;
            pos.z = Mathf.Round(pos.z / planet.voxelSize) * planet.voxelSize;
            i = Voxel.PositionToIndex(pos, planet.voxelsPerChunk, planet.voxelSize);
            voxels[i].value = 0;
        }
        else
        {
            voxels[i].value = 0;
        }


        if (BlocksGone())
        {
            gameObject.SetActive(false);
        }
        else
        {
            buffer = 0;
            verts.Clear();
            tris.Clear();
            colors.Clear();

            CreateMeshData();
            UpdateMesh();
        }



    }
    
    void CreateVoxelData()
    {
        voxels = new Voxel[(int)Mathf.Pow(planet.voxelsPerChunk, 3)];
        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new Voxel();
            voxels[i].position = Voxel.IndexToPosition(i, planet.voxelsPerChunk, planet.voxelSize);

            float distanceFromCenter = Vector3.Distance(Vector3.one * planet.radius, transform.localPosition + voxels[i].position);

            if (distanceFromCenter > planet.radius)
            {
                continue;
            }
            else
            {
                float maxDistance = Vector3.Distance(Vector3.one * planet.radius, Voxel.IndexToPosition(0, planet.voxelsPerChunk, planet.voxelSize));
                float invertedDistanceFromCenter = Mathf.Abs(distanceFromCenter - maxDistance);
                voxels[i].value = invertedDistanceFromCenter / maxDistance;
                
                if(voxels[i].value < planet.threshold)
                {
                    voxels[i].color = Color.white;
                }
                else
                {
                    voxels[i].color = Color.black;
                }


                Vector3 index3d = Voxel.IndexToPosition(i, planet.voxelsPerChunk, 1);
                
                if (index3d.x <= 0 || index3d.y <= 0 || index3d.z <= 0 || index3d.x >= planet.voxelsPerChunk - 1 || index3d.y >= planet.voxelsPerChunk - 1 || index3d.z >= planet.voxelsPerChunk - 1)
                {
                    voxels[i].value = 0;
                    voxels[i].color = Color.black;
                }


            }

        }
    }

    void CreateMeshData()
    {
        int i = voxels.Length;
        while (i > 0)
        {
            Vector3 position = Voxel.IndexToPosition(i, planet.voxelsPerChunk, planet.voxelSize);
            Voxel[] points = new Voxel[]
            {
                    voxels[Voxel.PositionToIndex(position + new Vector3(0,0,-1),planet.voxelsPerChunk,planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position +  new Vector3(-1, 0, -1),planet.voxelsPerChunk, planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position +  new Vector3(-1, 0, 0),planet.voxelsPerChunk, planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position,planet.voxelsPerChunk,planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position + new Vector3(0, -1, -1), planet.voxelsPerChunk, planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position + new Vector3(-1, -1, -1), planet.voxelsPerChunk, planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position + new Vector3(-1, -1, 0), planet.voxelsPerChunk, planet.voxelSize)],
                    voxels[Voxel.PositionToIndex(position + new Vector3(0, -1, 0), planet.voxelsPerChunk, planet.voxelSize)]
            };

            int cubeIndex = Voxel.GetState(points, planet.isoLevel);
            int[] triangulation = MarchingCubesTables.triTable[cubeIndex];

            foreach (int edgeIndex in triangulation)
            {
                if (edgeIndex > -1)
                {
                    int a = MarchingCubesTables.edgeConnections[edgeIndex][0];
                    int b = MarchingCubesTables.edgeConnections[edgeIndex][1];

                    Vector3 vertexPos = Voxel.LerpPosition(points[a], points[b], planet.isoLevel);
                    colors.Add(Voxel.LerpColor(points[a], points[b], planet.isoLevel));

                    verts.Add(vertexPos);
                    tris.Add(buffer);
                    buffer++;
                }
                else
                {
                    break;
                }
            }
            i--;
        }
    }
    
   bool BlocksGone()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            if (voxels[i].value > planet.isoLevel)
            {
                return false;
            }
        }

        return true;
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.colors = colors.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
