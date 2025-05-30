using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Default Parameters")]
    [Tooltip("Controls how far apart everything is")][Min(1)] public float stepSize = 10;
    public string seed = string.Empty;

    [SerializeField] GameObject hallwayPrefab;
    [SerializeField] GameObject turnPrefab;
    [SerializeField] GameObject roomPrefab;

    [Space]
    [SerializeField][Min(1)] int minHallwayLength = 10;
    [SerializeField][Min(2)] int maxHallwayLength = 15;

    [Space]
    [SerializeField][Min(1)] int numberOfHalls = 5;

    Vector3 walkerPosition = Vector3.zero;
    Vector3 direction = Vector3.forward;
    Vector3 prevDirection = Vector3.forward;

    void Start()
    {
        Random.InitState(seed.GetHashCode());
        GenerateDungeon();
    }
    
    void GenerateDungeon()
    {
        for(int h = 0; h < numberOfHalls; h++)
        {
            CreateHallway();
        }

    }

    void CreateHallway()
    {
        int length = Random.Range(minHallwayLength, maxHallwayLength);
        for (int s = 0; s < length; s++)
        {
            if(prevDirection + direction != Vector3.zero) 
            {
                AutoTile();
                walkerPosition += direction * stepSize;
                prevDirection = direction;

                Vector3[] choices = { Vector3.forward, Vector3.right, Vector3.up, };
                direction = RandomDirection(choices);
            }
        }


    }

    void AutoTile()
    {
        if (direction == prevDirection)
        {
            if (direction == Vector3.forward || direction == Vector3.back)
            {
                Instantiate(hallwayPrefab, walkerPosition, Quaternion.Euler(0, 0, 0), transform);
            }
            else if (direction == Vector3.right || direction == Vector3.left)
            {
                Instantiate(hallwayPrefab, walkerPosition, Quaternion.Euler(0, 90, 0), transform);
            }
            else if(direction == Vector3.up || direction == Vector3.down)
            {
                Instantiate(hallwayPrefab, walkerPosition, Quaternion.Euler(90, 0, 0), transform);
            }
        }
        else
        {
            if (prevDirection == Vector3.forward && direction == Vector3.left)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 0, 180), transform);
            }
            else if (prevDirection == Vector3.forward && direction == Vector3.right)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 0, 0), transform);
            }
            else if (prevDirection == Vector3.forward && direction == Vector3.up)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 0, 90), transform);
            }
            else if (prevDirection == Vector3.forward && direction == Vector3.down)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 0, -90), transform);
            }

            else if (prevDirection == Vector3.back && direction == Vector3.left)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(180, -90, 0), transform);
            }
            else if (prevDirection == Vector3.back && direction == Vector3.right)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, -90, 0), transform);
            }
            else if (prevDirection == Vector3.back && direction == Vector3.up)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(90, -90, 0), transform);
            }
            else if (prevDirection == Vector3.back && direction == Vector3.down)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(-90, -90, 0), transform);
            }

            else if (prevDirection == Vector3.left && direction == Vector3.forward)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(180, 0, 0), transform);
            }
            else if (prevDirection == Vector3.left && direction == Vector3.back)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 0, 0), transform);
            }
            else if (prevDirection == Vector3.left && direction == Vector3.up)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(90, 0, 0), transform);
            }
            else if (prevDirection == Vector3.left && direction == Vector3.down)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(-90, 0, 0), transform);
            }

            else if (prevDirection == Vector3.right && direction == Vector3.forward)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 90, 180), transform);
            }
            else if (prevDirection == Vector3.right && direction == Vector3.back)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 90, 0), transform);
            }
            else if (prevDirection == Vector3.right && direction == Vector3.up)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 90, 90), transform);
            }
            else if (prevDirection == Vector3.right && direction == Vector3.down)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(0, 90, -90), transform);
            }

            else if (prevDirection == Vector3.up && direction == Vector3.forward)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(-90, 0, -90), transform);
            }
            else if (prevDirection == Vector3.up && direction == Vector3.back)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(-90, 0, 90), transform);
            }
            else if (prevDirection == Vector3.up && direction == Vector3.left)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(-90, 0, 180), transform);
            }
            else if (prevDirection == Vector3.up && direction == Vector3.right)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(-90, 0, 0), transform);
            }

            else if (prevDirection == Vector3.down && direction == Vector3.forward)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(90, 0, 90), transform);
            }
            else if (prevDirection == Vector3.down && direction == Vector3.back)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(90, 0, -90), transform);
            }
            else if (prevDirection == Vector3.down && direction == Vector3.left)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(90, 0, 180), transform);
            }
            else if (prevDirection == Vector3.down && direction == Vector3.right)
            {
                Instantiate(turnPrefab, walkerPosition, Quaternion.Euler(90, 0, 0), transform);
            }
        }
    }

    Vector3 RandomDirection(Vector3[] choices)
    {
        int d = Random.Range(0, choices.Length);
        return choices[d];
    }
}