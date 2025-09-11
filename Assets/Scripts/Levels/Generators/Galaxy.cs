using System.Collections.Generic;
using UnityEngine;

class GalaxyQuadrant
{
    public GalaxyQuadrant(int t, Vector3 pos)
    {
        position = pos;
        type = t;
    }

    public Vector3 position;
    public int type;
}

public class Galaxy : MonoBehaviour
{
    // For Procedural Generation
    public string seed = "One Piece Is Mid";
    [SerializeField][Min(1)] int spawnAsteroidFieldChance = 3;

    public static Noise noise;

    public float quadrantSize = 1000;
    public Vector2Int numberOfQuadrants = new Vector2Int(10,10);

    List<GalaxyQuadrant> quadrants;
    int levelIndex = 0;
    [SerializeField] GameObject[] levelPrefabs;
    [SerializeField] GameObject asteroidFieldPrefab;
    void Awake()
    {
        quadrants = new List<GalaxyQuadrant>();

        Random.InitState(seed.GetHashCode());
        noise = new Noise(seed.GetHashCode());
        Generate();
    }

    void Generate()
    {
        //Initialize Galaxy Quadrants
        for (int x = -numberOfQuadrants.x / 2; x <= numberOfQuadrants.x / 2; x++)
        {
            for (int z = -numberOfQuadrants.y / 2; z <= numberOfQuadrants.y / 2; z++)
            {
                float y = noise.Evaluate(new Vector3(x, 0, z));
                Vector3 quadrantPosition = new Vector3(x, y, z) * quadrantSize;
                quadrants.Add(new GalaxyQuadrant(0, quadrantPosition));
            }
        }

        // Set Levels Player Can Visit in Random Quadrants
        foreach(GameObject levels in levelPrefabs)
        {
            int randomIndex = Random.Range(0, quadrants.Count);
            quadrants[randomIndex].type = 1;
            var lvl = Instantiate(levelPrefabs[levelIndex], quadrants[randomIndex].position, Quaternion.identity, transform);
            lvl.name = levelPrefabs[levelIndex].name;
            levelIndex++;
        }

        //Add Asteroid Fields
        for(int i = 0; i < quadrants.Count; i++)
        {
            if (quadrants[i].type == 0)
            {
                int makeAsteroidField = Random.Range(0, spawnAsteroidFieldChance + 1);
                if(makeAsteroidField == 0)
                {
                    quadrants[i].type = 2;
                    Instantiate(asteroidFieldPrefab, quadrants[i].position , Quaternion.identity, transform);
                }
            }
        }

    }
}
