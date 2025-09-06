using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] string inputSeed = string.Empty;
    [SerializeField] float inputNoiseScale = 0.01f;
    [SerializeField] int inputChunkResolution = 10;
    [SerializeField] float inputVoxelSize = 10;

    public static Noise noise;
    public static string seed = string.Empty;
    public static float noiseScale;
    public static int chunkResolution;
    public static float voxelSize;


    [SerializeField] GameObject terrainChunkPrefab;
    [SerializeField][Min(0)] int xSize = 5;
    [SerializeField][Min(0)] int zSize = 10;

    [SerializeField][Min(1)] float spacingBetweenChunks = 90;

    public void Generate()
    {
        seed = inputSeed;
        noise = new Noise(seed.GetHashCode());
        noiseScale = inputNoiseScale;
        chunkResolution = inputChunkResolution;
        voxelSize = inputVoxelSize;


        if (transform.childCount > 0)
        {
            Debug.Log("Destroy Child  Objects Before Generating");
            return;
        }
        if (terrainChunkPrefab)
        {
            for (int x = 0; x <= xSize; x++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    GameObject chunkObject = Instantiate(terrainChunkPrefab, transform);
                    chunkObject.transform.position = new Vector3(x - (xSize / 2), 0, z - (zSize / 2)) * spacingBetweenChunks;
                    DestructibleTerrainChunk destructibleTerrain = chunkObject.GetComponent<DestructibleTerrainChunk>();
                    if (destructibleTerrain)
                    {
                        destructibleTerrain.Generate();
                    }
                    else
                    {
                        TerrainChunk terrain = chunkObject.GetComponent<TerrainChunk>();
                        if (terrain)
                        {
                            terrain.Generate();
                        }
                        else
                        {
                            Debug.Log("Prefab Does Not Have TerrainChunk.cs Or DestructibleTerrainChunk.cs Attached");
                            return;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Set Chunk Prefab");
            return;
        }

    }
}
