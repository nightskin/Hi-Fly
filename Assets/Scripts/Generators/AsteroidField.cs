using UnityEngine;

public class AsteroidField : MonoBehaviour
{
    [SerializeField] GameObject asteroidPrefab;
    [SerializeField] int minFieldDensity = 5;
    [SerializeField] int maxFieldDensity = 10;
    [SerializeField] float radius = 100;

    int fieldDensity = 10;

    void Start()
    {
        fieldDensity = Random.Range(minFieldDensity, maxFieldDensity);

        for (int i = 0; i < fieldDensity; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * radius;
            Instantiate(asteroidPrefab, pos, Quaternion.Euler(0, 0, 0), transform);
        }
    }

}
