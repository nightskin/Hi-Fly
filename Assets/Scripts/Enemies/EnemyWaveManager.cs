using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    static EnemyWaveManager instance;

    [SerializeField] ObjectPoolManager objectPool;

    [SerializeField] float minTime = 5;
    [SerializeField] float maxTime = 10;

    [SerializeField] int minEnemies = 10;
    [SerializeField] int maxEnemies = 20;

    public bool infinite = true;
    bool waveInProgress = false;
    public int enemiesInWave;
    [SerializeField] float timeBeforeNextWave;

    public static EnemyWaveManager Get()
    {
        if(instance)
        {
            return instance;
        }
        else
        {
            return null;
        }
    }
    
    bool WaveComplete()
    {
        if (enemiesInWave == 0)
        {
            return true;
        }
        return false;
    }

    public void StartWave()
    {
        enemiesInWave = Random.Range(minEnemies, maxEnemies);

        for (int i = 0; i < enemiesInWave; i++)
        {
            Vector3 AheadOfPlayer = GameManager.Get().playerObject.transform.position + (GameManager.Get().playerObject.transform.forward * 500);
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
        timeBeforeNextWave = Random.Range(minTime, maxTime);
    }
    
    void FixedUpdate()
    {
        if(!GameManager.Get().gameOver && !GameManager.Get().gamePaused)
        {
            if (waveInProgress)
            {
                if (WaveComplete())
                {
                    if(infinite) timeBeforeNextWave = Random.Range(minTime,maxTime);
                    waveInProgress = false;
                }
            }
            else
            {
                if (timeBeforeNextWave > 0)
                {
                    timeBeforeNextWave -= Time.fixedDeltaTime;
                }
                else
                {
                    StartWave();
                }
            }
        }
    }
}
