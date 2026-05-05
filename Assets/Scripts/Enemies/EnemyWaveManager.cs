using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    static EnemyWaveManager instance;

    [SerializeField] ObjectPoolManager objectPool;

    [SerializeField] float minTime = 5;
    [SerializeField] float maxTime = 20;

    [SerializeField] int maxEnemies = 10;
    [SerializeField] int minEnemies = 5;

    bool waveInProgress = false;
    float timeBeforeNextWave = 0;

    public static EnemyWaveManager Get()
    {
        return instance;
    }
    
    bool WaveComplete()
    {
        if (maxEnemies == 0)
        {
            return true;
        }
        return false;
    }

    public void StartWave()
    {
        int enemiesInWave = Random.Range(minEnemies, maxEnemies);

        for (int i = 0; i < enemiesInWave; i++)
        {
            Vector3 AheadOfPlayer = GameManager.Get().playerShip.transform.position + (GameManager.Get().playerShip.transform.forward * 500);
            objectPool.Spawn("enemy", AheadOfPlayer + Random.insideUnitSphere * 500);
        }
        waveInProgress = true;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        timeBeforeNextWave = Random.Range(minEnemies, maxEnemies+1);
    }
    
    void Update()
    {
        if(!GameManager.Get().gameOver && !GameManager.Get().gamePaused)
        {
            if (waveInProgress)
            {
                if (WaveComplete())
                {
                    timeBeforeNextWave = Random.Range(minTime,maxTime);
                    waveInProgress = false;
                }
            }
            else
            {
                if (timeBeforeNextWave > 0)
                {
                    timeBeforeNextWave -= Time.deltaTime;
                }
                else
                {
                    StartWave();
                }
            }
        }
    }
}
