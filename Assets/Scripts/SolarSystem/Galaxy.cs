using System.Collections.Generic;
using UnityEngine;

public class Galaxy : MonoBehaviour
{

    public static Noise noise;
    public float quadrantSize = 2000;
    public Vector2Int numberOfQuadrants = new Vector2Int(10,10);

    [SerializeField] GameObject planet;
    [SerializeField] GameObject asteroidField;
    [SerializeField] GameObject enemyBase;
    [SerializeField] GameObject enemySpawner;
    [SerializeField][Min(1)] int maxPlanets = 8;

    List<GameObject> quadrants = new List<GameObject>();
    int numberOfPlanets = 0;

    void Start()
    {
        noise = new Noise(GameManager.seed.GetHashCode());
        Random.InitState(GameManager.seed.GetHashCode());

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
                    // quadrant type == 2 means enemy base
                    // quadrant type == 3 means enemy fleet
                    // quadrant type == 4 means nothing

                    float y = noise.Evaluate(new Vector3(x, 0, z));
                    Vector3 quadrantPos = new Vector3(x, y, z) * quadrantSize;

                    if (quadrantType == 0 && numberOfPlanets < maxPlanets)
                    {
                        if (planet)
                        {
                            //Make Planet
                            GameObject p = Instantiate(planet, quadrantPos, Quaternion.identity, transform);
                            numberOfPlanets++;
                            p.name = numberOfPlanets.ToString();
                            quadrants.Add(p);
                        }
                    }
                    else if (quadrantType == 1)
                    {
                        if (asteroidField)
                        {
                            quadrants.Add(Instantiate(asteroidField, quadrantPos, Quaternion.identity, transform));
                        }
                    }
                    else if (quadrantType == 2)
                    {
                        //Make Enemy Base
                        if (enemyBase)
                        {
                            quadrants.Add(Instantiate(enemyBase, quadrantPos, Quaternion.identity, transform));
                        }
                    }
                    else if (quadrantType == 3)
                    {
                        if (enemySpawner)
                        {
                            var fleet = Instantiate(enemySpawner, quadrantPos, Quaternion.identity, transform);
                            fleet.name = "EnemyFleet";
                            quadrants.Add(fleet);
                        }
                    }
                }

            }
        }
    }

    void DrawVisible()
    {
        foreach (var quadrant in quadrants) 
        {
            if(quadrant.name == "EnemyFleet")
            {
                Vector3 camDirection = Camera.main.transform.forward;
                Vector3 toQuadrant = (quadrant.transform.position - Camera.main.transform.position).normalized;
                if (Vector3.Dot(camDirection, toQuadrant) > 0)
                {
                    quadrant.gameObject.SetActive(true);
                }
                else
                {
                    quadrant.gameObject.SetActive(false);
                }
            }

        }
    }
}
