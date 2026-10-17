using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Components
    ObjectPoolManager objectPool;
    [SerializeField] AudioSource sfx;
    public TrailRenderer trail;
    [SerializeField] AudioClip shootSound;

    // Bullet Atttributes
    public float intensity = 2.0f;
    public float lifetime = 5;
    public int power = 10;
    public float maxSpeed = 1000;
    public bool isPowerBomb = false;

    //Physics Variables
    [HideInInspector] public Transform homingTarget = null;
    [HideInInspector] public GameObject owner = null;
    [HideInInspector] public Vector3 direction;

    Vector3 prevPosition;
    bool hit;
    float life = 0;

    void Start()
    {
        objectPool = GameManager.Get().objectPool;    
    }
    void OnEnable()
    {
        trail.Clear();
        direction = Vector3.zero;
        hit = false;
        sfx.clip = shootSound;
        sfx.Play();
        trail.emitting = true;
        prevPosition = transform.position;
        life = lifetime;
    }
    void OnDisable()
    {
        homingTarget = null;
        trail.emitting = false;
    }

    void Update()
    {
        if(GameManager.Get().state == GameManager.GameState.GAME_PLAY)
        {
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                transform.position = Vector3.MoveTowards(transform.position, homingTarget.position, maxSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += direction * maxSpeed * Time.deltaTime;
            }

            //If bullet has not hit something check collisions
            if (!hit)
            {
                CheckCollisions();
            }
            else
            {
                if (!sfx.isPlaying)
                {
                    DeSpawn();
                }
            }

            //Destroy Bullet After A Certain Time has Past
            if (life > 0)
            {
                life -= Time.deltaTime;
            }
            else
            {
                DeSpawn();
            }
        }
    }
    void CheckCollisions()
    {
        if(homingTarget)
        {
            if(Vector3.Distance(homingTarget.transform.position, transform.position) < 1)
            {
                if(isPowerBomb) GameManager.Get().objectPool.Spawn("powerBomb", homingTarget.position);
                else GameManager.Get().objectPool.Spawn("sparks", homingTarget.position);
                HealthSystem health = homingTarget.GetComponent<HealthSystem>();
                if(health) health.TakeDamage(power); 
                hit = true;
            }
        }
        else if (Physics.Linecast(prevPosition, transform.position, out RaycastHit rayhit))
        {
            if (rayhit.transform.gameObject != owner)
            {
                if (rayhit.transform.tag == "Destructible")
                {
                    if(isPowerBomb) GameManager.Get().objectPool.Spawn("powerBomb", rayhit.point);
                    else GameManager.Get().objectPool.Spawn("sparks", rayhit.point);

                    Asteroid asteroid = rayhit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(rayhit);
                    }
                    PlanetChunk planet = rayhit.transform.GetComponent<PlanetChunk>();
                    if(planet)
                    {
                        planet.RemoveBlock(rayhit);
                    }
                    hit = true;
                }
                else if (rayhit.transform.tag == "Surface")
                {
                    if(isPowerBomb) GameManager.Get().objectPool.Spawn("powerBomb", rayhit.point);
                    else GameManager.Get().objectPool.Spawn("sparks", rayhit.point);
                    hit = true;
                }
                else if (rayhit.transform.tag == "Enemy" || rayhit.transform.tag == "Player")
                {
                    if(isPowerBomb) GameManager.Get().objectPool.Spawn("powerBomb", rayhit.point);
                    else GameManager.Get().objectPool.Spawn("sparks", rayhit.point);

                    HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(power);
                        if(health.IsDead())
                        {
                            EnemyShip enemyShip = rayhit.transform.GetComponent<EnemyShip>();
                            if(enemyShip) enemyShip.Die();
                        }
                    }
                    else
                    {
                        Debug.Log("Enemy Does Not Have Health Script");
                    }
                    hit = true;
                }
                else if (rayhit.transform.tag == "Reflective")
                {
                    homingTarget = null;
                    owner = null;
                    life = lifetime;
                    direction = Vector3.Reflect(direction, rayhit.normal);
                }
            }
        }
    }
    void DeSpawn()
    {
        gameObject.SetActive(false);
    }
}
