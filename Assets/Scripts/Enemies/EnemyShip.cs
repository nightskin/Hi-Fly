using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    ObjectPool objectPool;
    Transform target;

    public bool skipPatrol;

    [SerializeField] HealthSystem health;
    [SerializeField] LayerMask targetLayer;


    [SerializeField][Range(0,100)] float avoidanceWeight = 1;
    [SerializeField][Range(0,100)] float flockingWeight = 1;
    [SerializeField][Range(0, 100)] float huntWeight = 1;
 
    [SerializeField] float perceptionRadius = 100;
    [SerializeField] float cohesionRadius = 100;
    [SerializeField] float avoidRadius = 10;
    [SerializeField] int attackPower = 10;
    [SerializeField] float turnSpeed = 10;

    [SerializeField] float fireRate = 1;
    [SerializeField] float moveSpeed = 75;

    Vector3 direction;
    float shootTimer = 0;


    void Start()
    {
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
    }

    void OnEnable()
    {
        if(GameManager.difficulty == GameManager.Difficulty.EASY)
        {
            turnSpeed = 3;
        }
        else if(GameManager.difficulty == GameManager.Difficulty.NORMAL)
        {
            turnSpeed = 4;
        }
        else if(GameManager.difficulty == GameManager.Difficulty.HARD)
        {
            turnSpeed = 5;
        }
        target = GameManager.playerShip.transform;
        health = GetComponent<HealthSystem>();
        health.Heal(health.GetMaxHealth());
        //Set colors
        GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", Util.RandomColor()); 
        GetComponent<MeshRenderer>().materials[2].SetColor("_MainColor", Util.RandomColor()); 
    }
    
    void Update()
    {
        if (target && !GameManager.gamePaused)
        {
            if (health.IsAlive())
            {
                if (Vector3.Distance(transform.position, target.transform.position) <= perceptionRadius || health.HasBeenHitOnce() || skipPatrol)
                {
                    Fight_Boid();
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
            PlayerShip player = other.GetComponent<PlayerShip>();
            player.health.TakeDamage(attackPower);
            if (player.health.IsDead())
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
        huntWeight = Vector3.Distance(transform.position, target.position);
        huntWeight = Mathf.Clamp(huntWeight, 0, 100);
        direction = (SteerTowardsTarget() * huntWeight) + (Seperation(avoidRadius) * avoidanceWeight) + (Cohesion(cohesionRadius) * flockingWeight);
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            if (Physics.SphereCast(transform.position, 1 ,transform.forward, out RaycastHit hit, 1000, targetLayer))
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
