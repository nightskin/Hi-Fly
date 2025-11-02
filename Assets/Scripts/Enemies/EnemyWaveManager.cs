using TMPro;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    static EnemyWaveManager instance;
    static PlayerShip player;

    [SerializeField] ObjectPoolManager objectPool;
    [SerializeField] TextMeshProUGUI waveInfo;
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject upgradeMenu;

    [SerializeField] int startAmountOfEnemiesInWave = 10;
    [SerializeField] [Min(0)] float intervalBetweenWaves = 3.0f;
    [SerializeField] [Min(0)] int enemyIncrement = 1;

    
    int currentWaveNumber = 0;
    int enemiesInCurrentWave;
    
    int enemiesDownedInTotal = 0;
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

    void StartWave()
    {
        currentWaveNumber++;
        enemiesDownedInCurrentWave = 0;
        timeBeforeNextWave = intervalBetweenWaves;

        for (int i = 0; i < enemiesInCurrentWave; i++)
        {
            objectPool.Spawn("enemy", player.transform.position + (player.transform.forward * 500) + Random.insideUnitSphere * 500);
        }
        waveInProgress = true;
    }

    public void EnemyDowned()
    {
        enemiesDownedInTotal++;
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
        enemiesInCurrentWave = startAmountOfEnemiesInWave;
        player = GameManager.Get().playerShip;
        GameManager.Get().CloseUpgradeMenu();
        StartWave();
    }
    
    void Update()
    {
        if (waveInProgress)
        {
            if (WaveComplete())
            {
                enemiesInCurrentWave += enemyIncrement;
                waveInProgress = false;
                GameManager.Get().OpenUpgradeMenu();
            }
        }
        else
        {
            if(timeBeforeNextWave > 0 && !GameManager.Get().gamePaused)
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
