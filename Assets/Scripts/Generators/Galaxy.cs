using System.Collections.Generic;
using UnityEngine;

class GalaxyQuadrant
{
    public GalaxyQuadrant(QuadrantType t, Vector3 pos)
    {
        position = pos;
        type = t;
    }

    public Vector3 position;
    public enum QuadrantType
    {
        EMPTY,
        ASTEROID_FIELD,
        PLANET,
        BLACK_HOLE,
    }
    public QuadrantType type;
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


    [SerializeField] int numberOfPlanets = 10;
    [SerializeField] GameObject blackHolePrefab;
    [SerializeField] GameObject asteroidFieldPrefab;
    [SerializeField] GameObject planetPrefab;


    void Awake()
    {
        quadrants = new List<GalaxyQuadrant>();

        if(seed == string.Empty) seed = Random.value.ToString();

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
                GalaxyQuadrant.QuadrantType t = (GalaxyQuadrant.QuadrantType)Random.Range(0,4);
                quadrants.Add(new GalaxyQuadrant(t, quadrantPosition));
            }
        }

        
        for(int i = 0; i < quadrants.Count; i++)
        {
            if(quadrants[i].type == GalaxyQuadrant.QuadrantType.EMPTY) continue;
            else if(quadrants[i].type == GalaxyQuadrant.QuadrantType.ASTEROID_FIELD && asteroidFieldPrefab)
            {
                Instantiate(asteroidFieldPrefab, quadrants[i].position, Quaternion.identity, transform);
            }
            else if(quadrants[i].type == GalaxyQuadrant.QuadrantType.PLANET && planetPrefab)
            {
                Instantiate(planetPrefab, quadrants[i].position, Quaternion.identity, transform);
            }
            else if(quadrants[i].type == GalaxyQuadrant.QuadrantType.BLACK_HOLE && blackHolePrefab)
            {
                Instantiate(blackHolePrefab,quadrants[i].position,Quaternion.identity,transform);
            }
        }
    }
}
