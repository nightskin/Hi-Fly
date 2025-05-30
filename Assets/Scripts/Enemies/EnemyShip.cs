using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    ObjectPool objectPool;
    GameObject target;

    public bool attackMode;

    [SerializeField] HealthSystem health;

    [SerializeField] float aimSkill = 10;
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
            turnSpeed = 5;
        }
        else if(GameManager.difficulty == GameManager.Difficulty.HARD)
        {
            turnSpeed = 7;
        }
        target = GameManager.playerShip.gameObject;
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
                if (Vector3.Distance(transform.position, target.transform.position) <= perceptionRadius || health.HasBeenHitOnce() || attackMode)
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
                var explosion = GameObject.Find("ObjectPool").GetComponent<ObjectPool>().Spawn("explosion", transform.position);
                GameObject.Find("Player").GetComponent<GameManager>().AddScore(10);
                gameObject.SetActive(false);
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            PlayerShip player = other.GetComponent<PlayerShip>();
            if(!player.evading)
            {
                player.health.TakeDamage(attackPower);
                if (player.health.IsDead())
                {
                    GameManager.gameOver = true;
                }
                health.TakeDamage(health.GetMaxHealth());
            }
        }
        if(other.tag == "Destructible" || other.tag == "Surface")
        {
            health.TakeDamage(health.GetMaxHealth());
        }
    }


    void Patrol()
    {
        direction = Cohesion(cohesionRadius) + Seperation(avoidRadius);
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
    
    void Fight_Boid()
    {
        direction = Vector3.Lerp(transform.forward, GetDirectionTowardsTarget(), turnSpeed * Time.deltaTime).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            if (Physics.SphereCast(transform.position, aimSkill, transform.forward, out RaycastHit hit))
            {
                if(hit.transform.gameObject == target)
                {
                    Shoot();
                }
            }

        }
    }
    
    void Shoot()
    {
        var b = objectPool.Spawn("bullet", transform.position + transform.forward);
        b.GetComponent<Bullet>().direction = transform.forward;
        b.GetComponent<Bullet>().owner = gameObject;
        b.GetComponent<Bullet>().damage = attackPower;
        shootTimer = Random.Range(0.1f, fireRate);
    }
    
    Vector3 GetDirectionTowardsTarget()
    {
        return (target.transform.position - transform.position).normalized;
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
