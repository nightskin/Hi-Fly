using UnityEngine;
using UnityEngine.UI;

public class EnemyWaveManager : MonoBehaviour
{
    static EnemyWaveManager instance;

    [SerializeField] ObjectPoolManager objectPool;
    [SerializeField] Text waveInfo;
    [SerializeField] GameObject upgradeMenu;

    [SerializeField] int startAmountOfEnemiesInWave = 10;
    [SerializeField] [Min(0)] float intervalBetweenWaves = 3.0f;
    [SerializeField] [Min(0)] int enemyIncrement = 1;

    
    int currentWaveNumber = 1;
    int enemiesInCurrentWave;

    int enemiesDownedInCurrentWave = 0;

    bool waveInProgress = false;
    float timeBeforeNextWave = 0;

    public static EnemyWaveManager Get()
    {
        return instance;
    }
    
    bool WaveComplete()
    {
        if (enemiesDownedInCurrentWave == enemiesInCurrentWave)
        {
            return true;
        }
        return false;
    }

    public void StartWave()
    {
        for (int i = 0; i < enemiesInCurrentWave; i++)
        {
            Vector3 AheadOfPlayer = GameManager.Get().playerShip.transform.position + (GameManager.Get().playerShip.transform.forward * 500);
            objectPool.Spawn("enemy", AheadOfPlayer + Random.insideUnitSphere * 500);
        }
        waveInProgress = true;
    }

    public void EnemyDowned()
    {
        enemiesDownedInCurrentWave++;
        UpdateUI();
    }

    public void UpdateUI()
    {
        waveInfo.text = "Wave: " + currentWaveNumber.ToString() + " - Kills: " + enemiesDownedInCurrentWave.ToString() + "/" + enemiesInCurrentWave.ToString();
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        timeBeforeNextWave = intervalBetweenWaves;
        enemiesInCurrentWave = startAmountOfEnemiesInWave;
        UpdateUI();
    }
    
    void Update()
    {
        if(!GameManager.Get().gameOver && !GameManager.Get().gamePaused)
        {
            if (waveInProgress)
            {
                if (WaveComplete())
                {
                    currentWaveNumber++;
                    timeBeforeNextWave = intervalBetweenWaves;
                    enemiesInCurrentWave += enemyIncrement;
                    waveInProgress = false;
                    enemiesDownedInCurrentWave = 0;
                    GameManager.Get().OpenUpgradeMenu();
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
