using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    [SerializeField] GameObject pickUpPrefab;
    [SerializeField] EnemyDissolveEffect effect;

    ObjectPool objectPool;
    Transform target;

    [SerializeField] HealthSystem health;
    [SerializeField] LayerMask targetLayer;
    [SerializeField][Range(0, 1)] float shootThreshold = 0.75f;
    [SerializeField] float cohesionRadius = 100;
    [SerializeField] float avoidRadius = 10;
    [SerializeField] int attackPower = 10;
    [SerializeField] float turnSpeed = 10;

    [SerializeField] float fireRate = 1;
    [SerializeField] float moveSpeed = 75;

    Vector3 direction;
    float shootTimer = 0;

    bool diedByCrashing;

    void Start()
    {
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
    }

    void OnEnable()
    {
        diedByCrashing = false;
        effect.enabled = true;
        if (GameManager.difficulty == GameManager.Difficulty.EASY)
        {
            
        }
        else if (GameManager.difficulty == GameManager.Difficulty.NORMAL)
        {
            
        }
        else if (GameManager.difficulty == GameManager.Difficulty.HARD)
        {
            
        }
        target = GameManager.playerShip.transform.GetChild(0);
        health = GetComponent<HealthSystem>();
        health.Heal(health.GetMaxHealth());
        //Set colors
        GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", Util.RandomColor()); 
        GetComponent<MeshRenderer>().materials[2].SetColor("_MainColor", Util.RandomColor()); 
    }
    
    void Update()
    {
        if (!GameManager.gamePaused)
        {
            if (health.IsAlive())
            {
                if (GameManager.playerMode == GameManager.PlayerMode.ON_RAILS)
                {
                    FightOnRails();
                }
                else
                {
                    Fight();
                }
            }
            else
            {
                Die();
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            playerHealth.TakeDamage(10);
            if (playerHealth.IsDead())
            {
                GameManager.gameOver = true;
            }
            health.TakeDamage(health.GetMaxHealth());
            diedByCrashing = true;
        }
        if (other.tag == "Destructible" || other.tag == "Surface")
        {
            health.TakeDamage(health.GetMaxHealth());
            diedByCrashing = true;
        }
    }

    void Die()
    {
        var explosion = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().Spawn("explosion", transform.position);
        if (Util.RandomBool() && !diedByCrashing)
        {
            GameObject.Find("ObjectPool").GetComponent<ObjectPool>().Spawn("pickup", transform.position);
        }
        gameObject.SetActive(false);

    }

    void Fight()
    {
        direction = SteerTowardsTarget() + Seperation(avoidRadius) + Cohesion(cohesionRadius);
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0 && !effect.enabled)
        {
            if (Vector3.Dot(GetDirectionTowardsTarget(), transform.forward) > shootThreshold)
            {
                Shoot();
            }
        }
    }

    void FightOnRails()
    {
        if (Vector3.Dot(Camera.main.transform.forward, transform.forward) > 0)
        {
            direction = SteerTowardsTarget();
            transform.rotation = Quaternion.LookRotation(direction);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0 && !effect.enabled)
            {
                if (Vector3.Dot(GetDirectionTowardsTarget(), transform.forward) > shootThreshold)
                {
                    Shoot();
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void Shoot()
    {
        var b = objectPool.Spawn("bullet", transform.position + transform.forward);
        b.GetComponent<Bullet>().direction = GetDirectionTowardsTarget();
        b.GetComponent<Bullet>().owner = gameObject;
        b.GetComponent<Bullet>().damage = attackPower;
        shootTimer = Random.Range(0.1f, fireRate);
    }
    
    Vector3 GetDirectionTowardsTarget()
    {
        return (target.transform.position - transform.position).normalized;
    }
    
    Vector3 SteerTowardsTarget()
    {
        return Vector3.Lerp(transform.forward, GetDirectionTowardsTarget(), turnSpeed * Time.deltaTime).normalized;
    }

    Vector3 Cohesion(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        Vector3 rot = new Vector3();
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                rot += collider.transform.position;
            }
            rot /= colliders.Length;
            return Vector3.Lerp(transform.forward, (rot - transform.position).normalized, turnSpeed * Time.deltaTime).normalized;
        }
        return Vector3.zero;
    }

    Vector3 Seperation(float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        Vector3 steer = transform.forward;
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                if (Vector3.Dot(transform.forward, collider.transform.forward) >= 0)
                {
                    Vector3 pos = collider.transform.position;
                    steer += pos;
                }
            }
            steer /= colliders.Length;
            return Vector3.Lerp(transform.forward, (transform.position - steer).normalized, turnSpeed * Time.deltaTime).normalized;
        }
        return transform.forward;
    }
}
