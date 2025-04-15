using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour
{
    public float quadrantSize = 2000;
    public Vector2Int numberOfQuadrants = new Vector2Int(10,10);

    [SerializeField] GameObject[] planetPrefabs;
    [SerializeField] GameObject enemyBasePrefab;
    [SerializeField] GameObject asteroidFieldPrefab;
    [SerializeField] GameObject enemyFleatPrefab;

    List<GameObject> quadrants = new List<GameObject>();
    int numberOfPlanets = 0;

    void Start()
    {
        GameManager.InitRandom();
        Generate();
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
                    // quadrant type == 2 means Enemy fleet
                    // quadrant type == 3 means Enemy Base
                    // quadrant type == 4 means nothing 

                    float y = GameManager.noise.Evaluate(new Vector3(x, 0, z));
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
                                p.name = numberOfPlanets.ToString();
                                quadrants.Add(p);
                            }
                        }
                    }
                    else if (quadrantType == 1)
                    {
                        if (asteroidFieldPrefab)
                        {
                            quadrants.Add(Instantiate(asteroidFieldPrefab, quadrantPos, Quaternion.identity, transform));
                        }
                    }
                    else if (quadrantType == 2)
                    {
                        if (enemyFleatPrefab)
                        {
                            var fleet = Instantiate(enemyFleatPrefab, quadrantPos, Quaternion.identity, transform);
                            fleet.name = "EnemyFleet";
                            quadrants.Add(fleet);
                        }
                    }
                    else if(quadrantType == 3)
                    {
                        if (enemyBasePrefab)
                        {
                            quadrants.Add(Instantiate(enemyBasePrefab, quadrantPos, Quaternion.identity, transform));
                        }
                    }
                }
            }
        }
    }
}
