using System.Collections.Generic;
using System.Threading.Tasks;
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

    List<GameObject> quadrants = new List<GameObject>();

    void Start()
    {
        noise = new Noise(GameManager.seed.GetHashCode());
        Random.InitState(GameManager.seed.GetHashCode());

        Generate();
    }

    void FixedUpdate()
    {
        DrawVisible();
    }

    void Generate()
    {
        for (int x = 0; x < numberOfQuadrants.x; x++)
        {
            for (int z = 0; z < numberOfQuadrants.y; z++)
            {
                if (x != 0 || z != 0)
                {
                    int quadrantType = Mathf.RoundToInt(Random.value * 4);
                    // quadrant type == 0 means Planet
                    // quadrant type == 1 means Asteroid Field
                    // quadrant type == 2 means enemy base
                    // qaudrant type == 3 means enemies
                    // quadrant type == 4 means nothing

                    float y = noise.Evaluate(new Vector3(x, 0, z));
                    Vector3 quadrantPos = new Vector3(x - numberOfQuadrants.x / 2, y, z - numberOfQuadrants.y / 2) * quadrantSize;

                    if (quadrantType == 0)
                    {
                        if (planet)
                        {
                            //Make Planet
                            quadrants.Add(Instantiate(planet, quadrantPos, Quaternion.identity, transform));
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
                            quadrants.Add(Instantiate(enemySpawner, quadrantPos, Quaternion.identity, transform));
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
            Vector3 camDirection = Camera.main.transform.forward;
            Vector3 toQuadrant = (quadrant.transform.position - Camera.main.transform.position).normalized;
            if(Vector3.Distance(Camera.main.transform.position, quadrant.transform.position) <= Camera.main.farClipPlane + quadrantSize)
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
