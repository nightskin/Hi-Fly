using UnityEngine;

public class Bullet : MonoBehaviour
{
    AudioSource sfx;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;
    public float range = 2;
    public GameObject owner = null;
    public int damage = 10;
    public Transform homingTarget = null;
    public float speed = 1000;
    public Vector3 direction;

    ObjectPool explosionPool;
    Vector3 prevPosition;

    void Start()
    {
        explosionPool = GameObject.Find("ExplosionPool").GetComponent<ObjectPool>();
    }

    void OnEnable()
    {
        sfx = GetComponent<AudioSource>();
        sfx.clip = shootSound;
        sfx.Play();
        homingTarget = null;
        direction = Vector3.zero;
        GetComponent<TrailRenderer>().Clear();
        GetComponent<TrailRenderer>().emitting = true;
        prevPosition = transform.position;
        range = 2;
    }

    void Update()
    {
        //Basic Movement
        prevPosition = transform.position;
        if (homingTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, homingTarget.position, speed * Time.deltaTime);
        }
        else
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        //Exactly what is looks like
        CheckCollisions();

        //Destroy Bullet After A Certain Time has Past

        if (range > 0)
        {
            range -= Time.deltaTime;
        }
        else
        {
            DeSpawn();
        }
    }
    
    void CheckCollisions()
    {
        if (Physics.Linecast(prevPosition, transform.position, out RaycastHit hit))
        {
            if (hit.transform.gameObject != owner)
            {
                if (hit.transform.tag == "Asteroid")
                {
                    Asteroid asteroid = hit.transform.GetComponent<Asteroid>();
                    explosionPool.Spawn(hit.point);
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(hit);
                    }
                    DeSpawn();
                }
                else if (hit.transform.tag == "Planet")
                {
                    explosionPool.Spawn(hit.point);
                    DeSpawn();
                }
                else if (hit.transform.tag == "Enemy")
                {
                    HealthSystem health = hit.transform.GetComponent<HealthSystem>();
                    sfx.clip = hitSound;
                    sfx.Play();
                    if (health)
                    {
                        health.TakeDamage(damage);
                    }
                    DeSpawn();
                }
                else if (hit.transform.tag == "Player")
                {
                    PlayerShip player = hit.transform.GetComponent<PlayerShip>();
                    if(player)
                    {
                        if (player.evading)
                        {
                            owner = hit.transform.gameObject;
                            direction *= -1;
                        }
                        else
                        {
                            sfx.clip = hitSound;
                            sfx.Play();
                            HealthSystem health = hit.transform.GetComponent<HealthSystem>();
                            if (health)
                            {
                                health.TakeDamage(damage);
                                if (health.IsDead())
                                {
                                    explosionPool.Spawn(hit.transform.position);
                                    hit.transform.gameObject.SetActive(false);
                                    GameManager.gameOver = true;
                                }
                            }

                            DeSpawn();
                        }
                    }
                    else
                    {
                        Debug.Log("Player Script Not Found");
                        DeSpawn();
                    }
                }

            }
        }
    }

    void DeSpawn()
    {
        GetComponent<TrailRenderer>().emitting = false;
        gameObject.SetActive(false);
    }
}
