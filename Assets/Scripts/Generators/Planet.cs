using UnityEngine;

public class Planet : MonoBehaviour
{
    [SerializeField] GameObject chunkPrefab;
    [Range(0,1)] public float threshold = 0.6f;

    [HideInInspector] public Color landColor;
    [HideInInspector] public Color WaterColor;
    [HideInInspector] public int chunkResolution = 2;
    [HideInInspector] public float radius;
    [HideInInspector] public float voxelSize = 10;
    [HideInInspector] public int voxelsPerChunk = 10;
    [HideInInspector] public float isoLevel = 0.5f;

    PlanetChunk[] chunks;

    void Start()
    {
        Generate();    
    }

    public void Generate()
    {
        landColor = Util.RandomColor();
        WaterColor = Util.RandomColor();
        int r = Random.Range(1, 4);
        radius = r * 100;

        if(radius == 100)
        {
            voxelSize = 13;
            voxelsPerChunk = 10;
        }
        else if(radius == 200)
        {
            voxelSize = 15;
            voxelsPerChunk = 15;
        }
        else if(radius == 300)
        {
            voxelSize = 20;
            voxelsPerChunk = 17;
        }


        chunks = new PlanetChunk[(int)Mathf.Pow(chunkResolution, 3)];
        for(int i = 0; i < chunks.Length; i++)
        {
            var obj = Instantiate(chunkPrefab, transform);
            obj.transform.localPosition = Voxel.IndexToPosition(i, chunkResolution, voxelSize * (voxelsPerChunk - 3));
            chunks[i] = obj.GetComponent<PlanetChunk>();
        }


        foreach (PlanetChunk chunk in chunks)
        {
            chunk.Generate();
        }

    }


}
