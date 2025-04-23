using UnityEngine;

public class EnemyShip : MonoBehaviour
{

    GameObject target;
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


    [SerializeField] float turnFrequncy = 3.0f;
    float turnTimer = 0;

    void OnEnable()
    {
        target = GameObject.FindGameObjectWithTag("Player");
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
                if (Vector3.Distance(transform.position, target.transform.position) <= perceptionRadius || health.HasBeenHitOnce())
                {
                    if(GameManager.difficulty == GameManager.Difficulty.HARD)
                    {
                        Fight_Boid();
                    }
                    else
                    {
                        Fight_TimeBased();
                    }
                }
                else
                {
                    Patrol();
                }
            }
            else
            {
                GameObject.Find("ExplosionPool").GetComponent<ObjectPool>().Spawn(transform.position);
                gameObject.SetActive(false);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
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
        if(other.tag == "Astroid" || other.tag == "Planet")
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
        direction = SteerTowardsTarget() + Seperation(avoidRadius);
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            if (Physics.SphereCast(transform.position, aimSkill , transform.forward, out RaycastHit hit))
            {
                if(hit.transform.gameObject == target)
                {
                    Shoot();
                }
            }

        }

    }

    void Fight_TimeBased()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        if (turnTimer < 0)
        {
            turnTimer = Random.Range(0, turnFrequncy);
            direction = GetDirectionTowardsTarget();
        }
        else
        {
            turnTimer -= Time.deltaTime;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            if (Physics.SphereCast(transform.position, aimSkill, transform.forward, out RaycastHit hit))
            {
                if (hit.transform.gameObject == target)
                {
                    Shoot();
                }
            }

        }
    }

    void Shoot()
    {
        var b = GameObject.Find("BulletPool").GetComponent<ObjectPool>().Spawn(transform.position + transform.forward);
        b.GetComponent<Bullet>().direction = direction;
        b.GetComponent<Bullet>().owner = gameObject;
        b.GetComponent<Bullet>().damage = attackPower;
        shootTimer = Random.Range(0.1f, fireRate);
    }

    Vector3 SteerTowardsTarget()
    {
        return Vector3.Lerp(transform.forward, GetDirectionTowardsTarget(), turnSpeed * Time.deltaTime);
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
            return Vector3.Lerp(transform.forward, (rot - transform.position).normalized, turnSpeed * Time.deltaTime);
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
            return Vector3.Lerp(transform.forward, (transform.position - rot).normalized, turnSpeed * Time.deltaTime);
        }
        return Vector3.zero;
    }
}
