using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    [SerializeField] GameObject pickUpPrefab;
    [SerializeField] EnemyDissolveEffect effect;

    ObjectPool objectPool;
    Transform target;

    public bool skipPatrol;

    [SerializeField] HealthSystem health;
    [SerializeField] LayerMask targetLayer;


    [SerializeField][Range(0,100)] float avoidanceWeight = 50;
    [SerializeField][Range(0,100)] float flockingWeight = 50;
    [SerializeField] float aimSkill = 4;
 
    [SerializeField] float perceptionRadius = 100;
    [SerializeField] float cohesionRadius = 100;
    [SerializeField] float avoidRadius = 10;
    [SerializeField] int attackPower = 10;
    [SerializeField] float turnSpeed = 10;

    [SerializeField] float fireRate = 1;
    [SerializeField] float moveSpeed = 75;

    Vector3 direction;
    float shootTimer = 0;

    bool aggro;
    float turnTimer = 0;
    float turnFrequency;

    void Start()
    {
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
    }

    void OnEnable()
    {
        effect.enabled = true;
        aggro = Util.RandomBool();
        if (GameManager.difficulty == GameManager.Difficulty.EASY)
        {
            turnSpeed = 5;
            turnFrequency = 5;
        }
        else if (GameManager.difficulty == GameManager.Difficulty.NORMAL)
        {
            turnSpeed = 10;
            turnFrequency = 2.5f;
        }
        else if (GameManager.difficulty == GameManager.Difficulty.HARD)
        {
            turnSpeed = 15;
            turnFrequency = 1.25f;
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
                if (Vector3.Distance(transform.position, target.transform.position) <= perceptionRadius || health.HasBeenHitOnce() || skipPatrol)
                {
                    if (aggro) Fight_Boid();
                    else Fight_Time_Based();
                }
                else
                {
                    Patrol();
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
        if(other.tag == "Player")
        {
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            playerHealth.TakeDamage(10);
            if (playerHealth.IsDead())
            {
                GameManager.gameOver = true;
            }
            health.TakeDamage(health.GetMaxHealth());
        }
        if(other.tag == "Destructible" || other.tag == "Surface")
        {
            health.TakeDamage(health.GetMaxHealth());
        }
    }

    void Die()
    {
        var explosion = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().Spawn("explosion", transform.position);
        if (Util.RandomBool())
        {
            GameObject.Find("ObjectPool").GetComponent<ObjectPool>().Spawn("pickup", transform.position);
        }
        gameObject.SetActive(false);

    }

    void Patrol()
    {
        direction = (Cohesion(cohesionRadius) * flockingWeight) + (Seperation(avoidRadius) * avoidanceWeight);
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
    
    void Fight_Boid()
    {
        direction = SteerTowardsTarget();
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0 && !effect.enabled)
        {
            if (Physics.SphereCast(transform.position, aimSkill,transform.forward, out RaycastHit hit, 1000, targetLayer))
            {
                Shoot();
            }

        }
    }

    void Fight_Time_Based()
    {
        if (turnTimer <= 0)
        {
            direction = GetDirectionTowardsTarget();
            turnTimer = Random.Range(0.0f, turnFrequency);
        }
        else
        {
            turnTimer -= Time.deltaTime;
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), 10 * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;


        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0 && !effect.enabled)
        {
            if (Physics.SphereCast(transform.position, aimSkill,transform.forward, out RaycastHit hit, 1000, targetLayer))
            {
                Shoot();
            }

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

        Vector3 rot = new Vector3();
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                Vector3 pos = collider.transform.position;
                rot += pos;
            }
            rot /= colliders.Length;
            return Vector3.Lerp(transform.forward, (transform.position - rot).normalized, turnSpeed * Time.deltaTime).normalized;
        }
        return Vector3.zero;
    }
}
