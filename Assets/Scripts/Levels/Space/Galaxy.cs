using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour
{
    public string seed = "Dota2 < LOL";
    public Noise noise;
    public float quadrantSize = 2000;
    public Vector2Int numberOfQuadrants = new Vector2Int(10,10);

    [SerializeField] GameObject[] planetPrefabs;
    [SerializeField] GameObject enemyBasePrefab;
    [SerializeField] GameObject asteroidFieldPrefab;
    [SerializeField] GameObject enemyFleatPrefab;

    List<GameObject> quadrants = new List<GameObject>();
    int numberOfPlanets = 0;

    //For Random Encounters
    [SerializeField] Transform player;
    [SerializeField][Min(0)] float minEncounterTime = 3.0f;
    [SerializeField][Min(1)] float maxEncounterTime = 10.0f;
    [SerializeField] GameObject enemySpawnerPrefab;

    GameObject spawner;
    [SerializeField] float secondsBeforeEncounter;
    bool encounterInProgress = false;
    [SerializeField] bool enableEncounters;

    [SerializeField] List<Vector3> path = new List<Vector3>();
    [SerializeField] float pointSize = 50;
    [SerializeField] Color pointColor = new Color(1, 0.5f, 0);

    void OnDrawGizmos()
    {
        if(path.Count > 0) 
        {
            foreach(var point in path) 
            {
                Gizmos.color = pointColor;
                Gizmos.DrawSphere(point, pointSize);
            }
        }    
    }

    void Start()
    {
        GameManager.playerPath = path;
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
                    Vector3 position = player.transform.position + (player.transform.forward * 1000);
                    spawner = Instantiate(enemyFleatPrefab, position, Quaternion.identity);
                    spawner.GetComponent<EnemySpawner>().skipPatrol = true;
                    secondsBeforeEncounter = Random.Range(minEncounterTime, maxEncounterTime);
                }
            }
        }

    }
    void Generate()
    {
        for (int x = -numberOfQuadrants.x/2; x <= numberOfQuadrants.x/2; x++)
        {
            for (int z = -numberOfQuadrants.y/2; z <= numberOfQuadrants.y/2; z++)
            {
                if (x != 0 || z != 0)
                {
                    int quadrantType = Mathf.RoundToInt(Random.value * 4);
                    // quadrant type == 0 means Planet
                    // quadrant type == 1 means Asteroid Field
                    // quadrant type == 2 means Enemy Base
                    // quadrant type == 3 && 4 means nothing 

                    float y = noise.Evaluate(new Vector3(x, 0, z));
                    Vector3 quadrantPos = new Vector3(x, y, z) * quadrantSize;
                    
                    if (quadrantType == 0)
                    {
                        if (planetPrefabs.Length > 0)
                        {
                            //Make Planet
                            if(numberOfPlanets < planetPrefabs.Length)
                            {
                                GameObject p = Instantiate(planetPrefabs[numberOfPlanets], quadrantPos, Quaternion.identity, transform);
                                numberOfPlanets++;
                                p.name = new Vector3(x, 0, z).ToString();
                                quadrants.Add(p);
                            }
                        }
                    }
                    else if (quadrantType == 1)
                    {
                        if (asteroidFieldPrefab)
                        {
                            var q = Instantiate(asteroidFieldPrefab, quadrantPos, Quaternion.identity, transform);
                            q.name = new Vector3(x, 0 , z).ToString();
                            quadrants.Add(q);

                        }
                    }
                    else if(quadrantType == 2)
                    {
                        if (enemyBasePrefab)
                        {
                            var q = Instantiate(enemyBasePrefab, quadrantPos, Quaternion.identity, transform);
                            q.name = new Vector3(x, 0, z).ToString();
                            quadrants.Add(q);
                        }
                    }
                }
            }
        }
    }
}
