using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] int maxEnemies = 10;
    [SerializeField] int minEnemies = 3;
    [SerializeField] float spawnRadius = 500;

    public List<GameObject> enemyShips = new List<GameObject>();

    void Start()
    {
        int spawnAmount = Random.Range(minEnemies, maxEnemies + 1);
        for(int e = 0; e < spawnAmount; e++) 
        {
            if(enemyPrefabs.Length > 0) 
            {
                int i = Random.Range(0, enemyPrefabs.Length);
                Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
                var enemy = Instantiate(enemyPrefabs[i], pos, Util.RandomRotation(), transform);
                enemyShips.Add(enemy);
            }
            else
            {
                break;
            }
        }

    }

    public void ReSpawn(Vector3 pos)
    {
        foreach(GameObject enemy in  enemyShips) 
        {
            if(!enemy.activeSelf)
            {
                enemy.transform.position = pos;
                enemy.transform.rotation = Util.RandomRotation();
                enemy.SetActive(true);
            }
        }
    }

    
}
