using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;
    public float lifetime = 5;
    public GameObject owner = null;
    public int damage = 10;
    public Transform homingTarget = null;
    public float speed = 1000;
    public Vector3 direction;

    AudioSource sfx;
    TrailRenderer trail;
    ObjectPool explosionPool;
    Vector3 prevPosition;
    bool hit;
    float life = 0;

    void Awake()
    {
        life = lifetime;
        explosionPool = GameObject.Find("ExplosionPool").GetComponent<ObjectPool>();
        sfx = GetComponent<AudioSource>();
        trail = GetComponent<TrailRenderer>();
    }

    void OnEnable()
    {
        hit = false;
        sfx.clip = shootSound;
        sfx.Play();
        homingTarget = null;
        direction = Vector3.zero;
        trail.Clear();
        trail.emitting = true;
        prevPosition = transform.position;
        life = lifetime;
    }

    void Update()
    {
        if(!GameManager.gamePaused)
        {
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                transform.position = Vector3.MoveTowards(transform.position, homingTarget.transform.position, speed * Time.deltaTime);
            }
            else
            {
                transform.position += direction * speed * Time.deltaTime;
            }

            //If bullet has not hit something check collisions
            if(!hit)
            {
                CheckCollisions();
            }
            else
            {
                trail.emitting = false;
                if(!sfx.isPlaying)
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
        if (Physics.Linecast(prevPosition, transform.position, out RaycastHit rayhit))
        {
            if (rayhit.transform.gameObject != owner)
            {
                if (rayhit.transform.tag == "Destructible")
                {
                    explosionPool.Spawn(rayhit.point);
                    Asteroid asteroid = rayhit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(rayhit);
                        return;
                    }
                }
                else if (rayhit.transform.tag == "Surface")
                {
                    explosionPool.Spawn(rayhit.point);
                    return;
                }
                else if (rayhit.transform.tag == "Enemy")
                {
                    HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(damage);
                    }
                    hit = true;
                    sfx.clip = hitSound;
                    sfx.Play();
                    return;
                }
                else if (rayhit.transform.tag == "Player")
                {
                    PlayerShip player = rayhit.transform.GetComponent<PlayerShip>();
                    if(player)
                    {
                        if (player.evading)
                        {
                            owner = rayhit.transform.gameObject;
                            direction = Vector3.Reflect(direction, rayhit.normal);
                        }
                        else
                        {
                            HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                            if (health)
                            {
                                health.TakeDamage(damage);
                                if (health.IsDead())
                                {
                                    explosionPool.Spawn(rayhit.transform.position);
                                    rayhit.transform.gameObject.SetActive(false);
                                    GameManager.gameOver = true;
                                }
                            }
                            sfx.clip = hitSound;
                            sfx.Play();
                            hit = true;
                        }
                    }
                    return;
                }
            }
        }
    }

    void DeSpawn()
    {
        trail.emitting = false;
        gameObject.SetActive(false);
    }
}
