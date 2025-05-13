using UnityEngine;

public class AsteroidField : MonoBehaviour
{
    [SerializeField] GameObject asteroidPrefab;
    [SerializeField] int fieldDensity = 10;
    [SerializeField] float radius = 100;


    void Start()
    {
        for(int i = 0; i < fieldDensity; i++) 
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * radius;
            Instantiate(asteroidPrefab, pos, Quaternion.Euler(0, 0, 0), transform);
        }
    }

}
