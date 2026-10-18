using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Planet : MonoBehaviour
{
    float[] voxelGrid;
    
    public static int numThreads = 8;
    public ComputeShader marchingShader;
    ComputeBuffer trianglesBuffer;
    ComputeBuffer triangleCountBuffer;
    ComputeBuffer weightsBuffer;

    Color landColor;
    Color WaterColor;
    
    public float radius = 100;
    float voxelSize = 10;
    int voxelResolution = 32;
    public float isoLevel = 0.5f;


    void Start()
    {
        radius = Random.Range(100.0f, 166.0f);
        landColor = Random.ColorHSV();
        WaterColor = Random.ColorHSV();
        GetComponent<MeshRenderer>().material.SetColor("_LandColor", landColor);
        GetComponent<MeshRenderer>().material.SetColor("_WaterColor", WaterColor);

        CreateVoxelData();
        CreateMeshData();
    }

    void OnValidate()
    {
        CreateVoxelData();
        CreateMeshData();
    }

    public void Teraform(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
        pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
        pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
        int i = Voxel.PositionToIndex(pos, voxelResolution, voxelSize);

        //If voxel is already deactivated check the next one
        if (voxelGrid[i] <= isoLevel)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * voxelSize / 2));
            pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
            pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
            pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
            i = Voxel.PositionToIndex(pos, voxelResolution, voxelSize);
            
            voxelGrid[i] -= 15 * Time.deltaTime;
        }
        else
        {
            voxelGrid[i] -= 15 * Time.deltaTime;
        }


        if (GetComponent<MeshFilter>().mesh.vertexCount == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            CreateMeshData();
        }



    }
    
    public void RemoveBlock(RaycastHit hit)
    {
        Vector3 pos = transform.InverseTransformPoint(hit.point);
        pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
        pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
        pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
        int i = Voxel.PositionToIndex(pos, voxelResolution, voxelSize);

        //If voxel is already deactivated check the next one
        if (voxelGrid[i] <= isoLevel)
        {
            pos = transform.InverseTransformPoint(hit.point - (hit.normal * voxelSize / 2));
            pos.x = Mathf.Round(pos.x / voxelSize) * voxelSize;
            pos.y = Mathf.Round(pos.y / voxelSize) * voxelSize;
            pos.z = Mathf.Round(pos.z / voxelSize) * voxelSize;
            i = Voxel.PositionToIndex(pos, voxelResolution, voxelSize);
            
            voxelGrid[i] = -1;
        }
        else
        {
            voxelGrid[i] = -1;
        }


        if (GetComponent<MeshFilter>().mesh.vertexCount == 0)
        {
            Destroy(gameObject);
        }
        else
        {
            CreateMeshData();
        }



    }

    void CreateVoxelData()
    {
        voxelGrid = new float[(int)Mathf.Pow(voxelResolution, 3)];
        for (int i = 0; i < voxelGrid.Length; i++)
        {
            Vector3 voxelposition = Voxel.IndexToPosition(i, voxelResolution, voxelSize);
            Vector3 center = Vector3.one * radius;
            float distanceFromCenter = Vector3.Distance(center, voxelposition);

            if (distanceFromCenter > radius)
            {
                continue;
            }
            else
            {
                float maxDistance = Vector3.Distance(center, Vector3.zero);
                float invertedDistanceFromCenter = Mathf.Abs(distanceFromCenter - maxDistance);
                voxelGrid[i] = invertedDistanceFromCenter / maxDistance;
                
                Vector3 index3d = Voxel.IndexToPosition(i, voxelResolution, voxelSize);
                
                if (index3d.x == 0 || index3d.y == 0 || index3d.z == 0 || index3d.x == voxelResolution - 1 || index3d.y == voxelResolution - 1 || index3d.z == voxelResolution - 1)
                {
                    voxelGrid[i] = 0;
                }
            }
        }
    }

    void CreateMeshData()
    {
        trianglesBuffer = new ComputeBuffer(5 * voxelResolution * voxelResolution * voxelResolution, Triangle.SizeOf, ComputeBufferType.Append);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        weightsBuffer = new ComputeBuffer(voxelResolution * voxelResolution * voxelResolution, sizeof(float));

        GetComponent<MeshFilter>().mesh = GetComponent<MeshCollider>().sharedMesh = ConstructMesh(); 

        trianglesBuffer.Release();
        triangleCountBuffer.Release();
        weightsBuffer.Release();
    }
    
    Mesh CreateMeshFromTriangles(Triangle[] triangles, bool insideOut = false)
    {
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] indices = new int[triangles.Length * 3];
        List<Vector2> uvs = new List<Vector2>();

        if(insideOut)
        {
            for (int i = 0; i < triangles.Length; i++) 
            {
                int startIndex = i * 3; 
                vertices[startIndex] = triangles[i].a * voxelSize;
                vertices[startIndex + 1] = triangles[i].b * voxelSize;
                vertices[startIndex + 2] = triangles[i].c * voxelSize; 
                indices[startIndex] = startIndex;
                indices[startIndex + 1] = startIndex + 1;
                indices[startIndex + 2] = startIndex + 2;
                uvs.AddRange(Voxel.GetUVs(triangles[i].a, triangles[i].b, triangles[i].c,voxelSize));
            }
        }
        else
        {
            for (int i = 0; i < triangles.Length; i++) 
            {
                int startIndex = i * 3; 
                vertices[startIndex] = triangles[i].c * voxelSize;
                vertices[startIndex + 1] = triangles[i].b * voxelSize;
                vertices[startIndex + 2] = triangles[i].a * voxelSize; 
                indices[startIndex] = startIndex;
                indices[startIndex + 1] = startIndex + 1;
                indices[startIndex + 2] = startIndex + 2;
                uvs.AddRange(Voxel.GetUVs(triangles[i].c, triangles[i].b, triangles[i].a,voxelSize));
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh ConstructMesh()
    {
        marchingShader.SetBuffer(0,"triangles", trianglesBuffer);
        marchingShader.SetBuffer(0, "weights", weightsBuffer);
        int[] chunkSize = {voxelResolution, voxelResolution, voxelResolution};
        marchingShader.SetInts("chunkSize", chunkSize);
        marchingShader.SetFloat("isoLevel", isoLevel);
        marchingShader.SetInt("style", 2);
        weightsBuffer.SetData(voxelGrid);
        trianglesBuffer.SetCounterValue(0);

        marchingShader.Dispatch(0, voxelResolution / numThreads, voxelResolution / numThreads, voxelResolution / numThreads);
        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        
        trianglesBuffer.GetData(triangles);
        return CreateMeshFromTriangles(triangles);
    }

    int ReadTriangleCount()
    {
        int[] triCount = {0};
        ComputeBuffer.CopyCount(trianglesBuffer, triangleCountBuffer, 0);
        triangleCountBuffer.GetData(triCount);
        return triCount[0];
    }


}
