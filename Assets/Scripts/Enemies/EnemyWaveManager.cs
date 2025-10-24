using TMPro;
using UnityEngine;

public class EnemyWaveManager : MonoBehaviour
{
    static EnemyWaveManager instance;
    static PlayerShip player;

    public ObjectPool objectPool;
    public TextMeshProUGUI waveInfo;
    public GameObject enemyPrefab;
    public GameObject upgradeMenu;

    [SerializeField] int startAmountOfEnemiesInWave = 10;
    [SerializeField] [Min(0)] float intervalBetweenWaves = 3.0f;
    [SerializeField] [Min(0)] int enemyIncrement = 1;

    
    int currentWaveNumber = 1;
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
        waveInProgress = true;
        enemiesInCurrentWave = 0;
        timeBeforeNextWave = intervalBetweenWaves;

        for (int i = 0; i < enemiesInCurrentWave; i++)
        {
            objectPool.Spawn("enemy", player.transform.position + (player.transform.forward * 500) + Random.insideUnitSphere * 500);
        }
    }

    public void EnemyDowned()
    {
        enemiesDownedInTotal++;
        enemiesDownedInCurrentWave++;
        waveInfo.text = "Wave: " + currentWaveNumber.ToString() + " - Kills: " + enemiesDownedInCurrentWave.ToString() + "/" + enemiesInCurrentWave.ToString();
    }

    void Awake()
    {
        instance = this;    
    }

    void Start()
    {
        GameManager.Get().CloseUpgradeMenu();
        StartWave();
    }
    
    void Update()
    {
        if (waveInProgress)
        {
            if (WaveComplete())
            {
                currentWaveNumber++;
                waveInProgress = false;
                enemiesInCurrentWave += enemyIncrement;
                GameManager.Get().OpenUpgradeMenu();
            }
            else
            {

            }
        }
        else
        {
            if(timeBeforeNextWave > 0)
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
