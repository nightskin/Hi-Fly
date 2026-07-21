using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Components
    ObjectPoolManager objectPool;
    [SerializeField] AudioSource sfx;
    public TrailRenderer trail;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;

    // Bullet Atttributes
    public float intensity = 2.0f;
    public float lifetime = 5;
    public int damage = 10;
    public float maxSpeed = 1000;
    public float blastRadius = 5;

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
    void FixedUpdate()
    {
        if(!GameManager.Get().gamePaused)
        {
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
        }
    }

    void Update()
    {
        if(!GameManager.Get().gamePaused)
        {
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                Vector3 targetDirection = (homingTarget.transform.position - transform.position).normalized;
                direction = Vector3.Lerp(direction, targetDirection, 10 * Time.deltaTime);
                float speed = Mathf.Lerp(0, maxSpeed, 20 * Time.deltaTime);
                transform.position += direction * speed * Time.deltaTime;
                if(!hit)
                {
                    if (Vector3.Distance(transform.position, homingTarget.position) < 1)
                    {
                        HealthSystem health = homingTarget.GetComponent<HealthSystem>();
                        if (health)
                        {
                            health.TakeDamage(damage);
                            hit = true;
                            sfx.clip = hitSound;
                            sfx.Play();
                        }

                        var obj = GameManager.Get().objectPool.Spawn("explosion", homingTarget.transform.position);
                    }
                }
            }
            else
            {
                transform.position += direction * maxSpeed * Time.deltaTime;
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
        if (Physics.Linecast(prevPosition, transform.position, out RaycastHit rayhit))
        {
            if (rayhit.transform.gameObject != owner)
            {
                if (rayhit.transform.tag == "Destructible")
                {
                    var obj = GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                    Asteroid asteroid = rayhit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(rayhit);
                        hit = true;
                    }
                    PlanetChunk planet = rayhit.transform.GetComponent<PlanetChunk>();
                    if(planet)
                    {
                        planet.RemoveBlock(rayhit);
                        hit = true;
                    }
                }
                else if (rayhit.transform.tag == "Surface")
                {
                    var obj = GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                    hit = true;
                }
                else if (rayhit.transform.tag == "Enemy")
                {
                    HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(damage);
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
                    
                    var obj = GameManager.Get().objectPool.Spawn("explosion", rayhit.point);

                    sfx.clip = hitSound;
                    sfx.Play();
                    hit = true;
                }
                else if (rayhit.transform.tag == "Player")
                {
                    HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(damage);
                        if (health.IsDead())
                        {
                            GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                            rayhit.transform.gameObject.SetActive(false);
                            GameManager.Get().gameOver = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Player Missing Health Script");
                    }

                    var obj = GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                    
                    hit = true;
                    sfx.clip = hitSound;
                    sfx.Play();
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
