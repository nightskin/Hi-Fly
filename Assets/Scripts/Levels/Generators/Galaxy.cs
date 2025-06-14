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
    public string seed = "";
    public Noise noise;
    public float quadrantSize = 1000;
    public Vector2Int numberOfQuadrants = new Vector2Int(10,10);

    List<GalaxyQuadrant> quadrants;
    int levelIndex = 0;
    [SerializeField] GameObject[] levelPrefabs;
    [SerializeField] GameObject asteroidFieldPrefab;
    
    //For Random Encounters
    [SerializeField] GameObject enemyFleatPrefab;
    Transform player;
    [SerializeField][Min(0)] float minEncounterTime = 3.0f;
    [SerializeField][Min(1)] float maxEncounterTime = 10.0f;
    [SerializeField] GameObject enemySpawnerPrefab;

    GameObject spawner;
    [SerializeField] float secondsBeforeEncounter;
    bool encounterInProgress = false;
    [SerializeField] bool enableEncounters;
    
    void Start()
    {
        quadrants = new List<GalaxyQuadrant>();
        secondsBeforeEncounter = Random.Range(minEncounterTime, maxEncounterTime);
        player = GameObject.FindWithTag("Player").transform;

        Random.InitState(seed.GetHashCode());
        noise = new Noise(seed.GetHashCode());
        Generate();
    }
    void Update()
    {
        if(enableEncounters)
        {
            if (encounterInProgress)
            {
                if (spawner.GetComponent<EnemySpawner>().AllEnemiesDefeated())
                {
                    Destroy(spawner);
                    encounterInProgress = false;
                }
            }
            else
            {
                secondsBeforeEncounter -= Time.deltaTime;
                if (secondsBeforeEncounter < 0)
                {
                    encounterInProgress = true;
                    Vector3 position = player.transform.position + (player.transform.forward * 500);
                    spawner = Instantiate(enemyFleatPrefab, position, Quaternion.identity);
                    spawner.GetComponent<EnemySpawner>().skipPatrol = true;
                    secondsBeforeEncounter = Random.Range(minEncounterTime, maxEncounterTime);
                }
            }
        }

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
                bool makeAsteroidField = Util.RandomBool();
                if(makeAsteroidField)
                {
                    quadrants[i].type = 2;
                    Instantiate(asteroidFieldPrefab, quadrants[i].position , Quaternion.identity, transform);
                }
            }
        }

    }
}
