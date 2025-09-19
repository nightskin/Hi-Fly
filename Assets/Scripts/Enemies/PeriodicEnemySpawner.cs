using UnityEngine;

public class PeriodicEnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyFleatPrefab;
    [SerializeField] int minEnemiesPerEncounter = 5;
    [SerializeField] int maxEnemiesPerEncounter = 10;
    [SerializeField][Min(0)] float minEncounterTime = 3.0f;
    [SerializeField][Min(1)] float maxEncounterTime = 10.0f;
    [SerializeField] float spawnRadius = 500;

    Transform player;

    GameObject currentSpawner = null;
    [SerializeField] float secondsBeforeEncounter;
    bool encounterInProgress = false;

    void OnEnable()
    {
        secondsBeforeEncounter = Random.Range(minEncounterTime, maxEncounterTime);
        player = GameObject.FindWithTag("Player").transform;
    }

    void OnDisable()
    {
        encounterInProgress = false;
        if (currentSpawner)
        {
            Destroy(currentSpawner);
            currentSpawner = null;
        }
    }

    void Update()
    {
        if (encounterInProgress)
        {
            if (currentSpawner.GetComponent<EnemySpawner>().AllEnemiesDefeated())
            {
                Destroy(currentSpawner);
                encounterInProgress = false;
            }
        }
        else
        {
            secondsBeforeEncounter -= Time.deltaTime;
            if (secondsBeforeEncounter < 0)
            {
                encounterInProgress = true;
                Vector3 position = player.transform.position + (player.transform.forward * spawnRadius * 2);
                currentSpawner = Instantiate(enemyFleatPrefab, position, Quaternion.identity);
                EnemySpawner spawner = currentSpawner.GetComponent<EnemySpawner>();
                spawner.skipPatrol = true;
                spawner.spawnAtStart = true;
                spawner.minEnemies = minEnemiesPerEncounter;
                spawner.maxEnemies = maxEnemiesPerEncounter;
                spawner.spawnRadius = spawnRadius;
                secondsBeforeEncounter = Random.Range(minEncounterTime, maxEncounterTime);
            }
        }
    }
}
