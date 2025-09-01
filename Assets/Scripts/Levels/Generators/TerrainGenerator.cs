using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public static string seed = string.Empty;

    [SerializeField] string inputSeed = string.Empty;


    [SerializeField] GameObject terrainChunkPrefab;
    [SerializeField][Min(0)] int xSize = 5;
    [SerializeField][Min(0)] int zSize = 10;

    [SerializeField][Min(1)] float spacingBetweenChunks = 90;

    [HideInInspector] public bool generated = false;

    void Start()
    {
        if (!generated) Generate();
    }

    public void Generate()
    {
        seed = inputSeed;
        if (transform.childCount > 0)
        {
            Debug.Log("Destroy Child  Objects Before Generating");
            return;
        }
        if (terrainChunkPrefab)
        {
            for (int x = 0; x < xSize; x++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    GameObject chunkObject = Instantiate(terrainChunkPrefab, transform);
                    chunkObject.transform.position = new Vector3(x - xSize / 2, 0, z - zSize / 2) * spacingBetweenChunks;
                    TerrainChunk terrain = chunkObject.GetComponent<TerrainChunk>();
                    if (terrain)
                    {
                        terrain.Generate();
                    }
                    else
                    {
                        Debug.Log("Chunk Object Does Not Have TerrainChunk.cs Script Attached");
                        generated = false;
                    }
                }
            }
            Debug.Log("Terrain Generation Successful");
            generated = true;
        }
        else
        {
            Debug.Log("Set Chunk Prefab");
            generated = false;
        }

    }
}
