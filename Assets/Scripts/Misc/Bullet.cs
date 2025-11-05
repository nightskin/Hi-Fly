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
    public float speed = 1000;
    public float blastRadius = 5;
    [HideInInspector] public bool explosive = false;

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
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                transform.position = Vector3.MoveTowards(transform.position, homingTarget.transform.position, speed * Time.fixedDeltaTime);
                if(!hit)
                {
                    if (Vector3.Distance(transform.position, homingTarget.position) < 1)
                    {
                        if(explosive)
                        {
                            var obj = objectPool.Spawn("powerBombExplosion", homingTarget.position);
                            PowerBomb bomb = obj.GetComponent<PowerBomb>();
                            if (bomb)
                            {
                                bomb.blastRadius = blastRadius;
                                bomb.damage = damage;
                            }
                            DeSpawn();
                        }
                        else
                        {
                            HealthSystem health = homingTarget.GetComponent<HealthSystem>();
                            if (health)
                            {
                                health.TakeDamage(damage);
                                hit = true;
                                sfx.clip = hitSound;
                                sfx.Play();
                            }
                        }

                    }
                }
            }
            else
            {
                transform.position += direction * speed * Time.fixedDeltaTime;
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
                life -= Time.fixedDeltaTime;
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
                if(explosive)
                {
                    if(rayhit.transform.tag == "Destructible")
                    {
                        var obj = objectPool.Spawn("powerBombExplosion", rayhit.point);
                        PowerBomb bomb = obj.GetComponent<PowerBomb>();
                        if (bomb)
                        {
                            bomb.blastRadius = blastRadius;
                            bomb.damage = damage;
                        }
                        Asteroid asteroid = rayhit.transform.GetComponent<Asteroid>();
                        if (asteroid)
                        {
                            asteroid.RemoveBlocksInRadius(rayhit, blastRadius / 2);
                        }

                    }
                    else if(rayhit.transform.tag == "Surface")
                    {
                        var obj = objectPool.Spawn("powerBombExplosion", rayhit.point);
                        PowerBomb bomb = obj.GetComponent<PowerBomb>();
                        if (bomb)
                        {
                            bomb.blastRadius = blastRadius;
                            bomb.damage = damage;
                        }
                    }
                    else if(rayhit.transform.tag == "Enemy")
                    {
                        var obj = objectPool.Spawn("powerBombExplosion", rayhit.point);
                        PowerBomb bomb = obj.GetComponent<PowerBomb>();
                        if (bomb)
                        {
                            bomb.blastRadius = blastRadius;
                            bomb.damage = damage;
                        }
                    }
                    else if(rayhit.transform.tag == "Drill")
                    {
                        owner = null;
                        life = lifetime;
                        homingTarget = null;
                        direction = Vector3.Reflect(direction, rayhit.normal);
                    }
                    else if(rayhit.transform.tag == "Player")
                    {
                        if(GameManager.Get().playerShip.evading)
                        {
                            homingTarget = null;
                            owner = rayhit.transform.gameObject;
                            life = lifetime;
                            direction = Vector3.Reflect(direction, rayhit.normal);
                        }
                        else
                        {
                            var obj = objectPool.Spawn("powerBombExplosion", rayhit.point);
                            PowerBomb bomb = obj.GetComponent<PowerBomb>();
                            if (bomb)
                            {
                                bomb.blastRadius = blastRadius;
                                bomb.damage = damage;
                            }
                        }
                    }
                    DeSpawn();
                }
                else
                {
                    if (rayhit.transform.tag == "Destructible")
                    {
                        GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                        Asteroid asteroid = rayhit.transform.GetComponent<Asteroid>();
                        if (asteroid)
                        {
                            asteroid.RemoveBlock(rayhit);
                            hit = true;
                        }
                        return;
                    }
                    else if (rayhit.transform.tag == "Surface")
                    {
                        GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                        hit = true;
                    }
                    else if (rayhit.transform.tag == "Enemy")
                    {
                        HealthSystem health = rayhit.transform.GetComponent<HealthSystem>();
                        if (health)
                        {
                            health.TakeDamage(damage);
                        }

                        sfx.clip = hitSound;
                        sfx.Play();
                        hit = true;
                    }
                    else if (rayhit.transform.tag == "Player")
                    {
                        if(GameManager.Get().playerShip.evading)
                        {
                            homingTarget = null;
                            owner = rayhit.transform.gameObject;
                            life = lifetime;
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
                                    GameManager.Get().objectPool.Spawn("explosion", rayhit.point);
                                    rayhit.transform.gameObject.SetActive(false);
                                    GameManager.Get().gameOver = true;
                                }
                            }
                            hit = true;
                            sfx.clip = hitSound;
                            sfx.Play();
                        }

                    }
                    else if (rayhit.transform.tag == "Drill")
                    {
                        homingTarget = null;
                        owner = null;
                        life = lifetime;
                        direction = Vector3.Reflect(direction, rayhit.normal);
                    }
                }
            }
        }
    }

    void DeSpawn()
    {
        gameObject.SetActive(false);
    }
}
