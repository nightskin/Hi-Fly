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
    public int power = 10;
    public float maxSpeed = 1000;
    public float blastRadius = 0;
    public bool directHoming = true;

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
        if(!GameManager.Get().gamePaused)
        {
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                if(directHoming)
                {
                    transform.position = Vector3.MoveTowards(transform.position, homingTarget.position, maxSpeed * Time.deltaTime);
                }
                else
                {
                    Vector3 targetDirection = (homingTarget.transform.position - transform.position).normalized;
                    direction = Vector3.Lerp(direction, targetDirection, 20 * Time.deltaTime);
                    transform.position += direction * maxSpeed * Time.deltaTime;
                }


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
            if(Vector3.Distance(homingTarget.transform.position, owner.transform.position) < 1)
            {
                HealthSystem health = homingTarget.GetComponent<HealthSystem>();
                if(health) health.TakeDamage(power); 
                if(blastRadius > 0) GameManager.Get().objectPool.Spawn("powerBomb", homingTarget.position);
                hit = true;
            }
        }

        if (Physics.Linecast(prevPosition, transform.position, out RaycastHit rayhit))
        {
            if (rayhit.transform.gameObject != owner)
            {
                if (rayhit.transform.tag == "Destructible")
                {
                    if(blastRadius > 0) GameManager.Get().objectPool.Spawn("powerBomb", rayhit.point);
                    else GameManager.Get().objectPool.Spawn("explosion", rayhit.point);

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
                }
                else if (rayhit.transform.tag == "Surface")
                {
                    if(blastRadius > 0) GameManager.Get().objectPool.Spawn("powerBomb", rayhit.point);
                    else GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                }
                else if (rayhit.transform.tag == "Enemy")
                {
                    if(blastRadius > 0) GameManager.Get().objectPool.Spawn("powerBomb", rayhit.transform.position);
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
                    sfx.clip = hitSound;
                    sfx.Play();
                    hit = true;
                }
                else if (rayhit.transform.tag == "Player")
                {
                    HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(power);
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
                hit = true;
            }
        }
    }
    void DeSpawn()
    {
        gameObject.SetActive(false);
    }
}
