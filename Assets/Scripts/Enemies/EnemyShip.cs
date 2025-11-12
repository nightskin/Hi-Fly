using UnityEngine;

public class EnemyShip : MonoBehaviour
{
    [SerializeField] EnemyDissolveEffect effect;
    [SerializeField] HealthSystem health;
    [SerializeField] LayerMask targetLayer;
    [SerializeField][Range(0, 1)] float shootThreshold = 0.75f;
    [SerializeField] float avoidRadius = 10;
    [SerializeField] int attackPower = 10;
    [SerializeField] float turnSpeed = 10;

    [SerializeField] float fireRate = 1;
    [SerializeField] float moveSpeed = 75;

    bool isSmart;
    Vector3 direction;
    float shootTimer = 0;
    float turnTimer;

    void OnEnable()
    {
        isSmart = true;
        effect.enabled = true;
        health = GetComponent<HealthSystem>();
        health.Heal(health.MaxHP());
        //Set colors
        GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", Util.RandomColor()); 
        GetComponent<MeshRenderer>().materials[2].SetColor("_MainColor", Util.RandomColor()); 
    }
    
    void Update()
    {
        if (!GameManager.Get().gamePaused)
        {
            if (health.IsAlive())
            {
                if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ON_RAILS)
                {
                    SteerOnRails();
                }
                else
                {
                    if (isSmart) Steer_Smart();
                    else Steer_Dumb();
                }

                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0 && !effect.enabled)
                {
                    if (Vector3.Dot(GetDirectionTowardsTarget(), transform.forward) > shootThreshold)
                    {
                        ShootAtPlayer();
                    }
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
                GameManager.Get().gameOver = true;
            }
            health.TakeDamage(health.MaxHP());
        }
        else if (other.tag == "Destructible" || other.tag == "Surface")
        {
            health.TakeDamage(health.MaxHP());
        }
        else if(other.tag == "Drill")
        {
            health.TakeDamage(health.MaxHP());
        }
    }

    void Die()
    {
        var explosion = GameManager.Get().objectPool.Spawn("explosion", transform.position);
        if ((GameManager.Get().gameMode == GameManager.GameMode.SURVIVOR))
        {
            EnemyWaveManager.Get().EnemyDowned();
        }
        gameObject.SetActive(false);
    }

    void Steer_Smart()
    {
        direction = SteerTowardsTarget() + (Seperation(avoidRadius));
        transform.rotation = Quaternion.LookRotation(direction);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    void Steer_Dumb()
    {
        if (turnTimer > 0)
        {
            turnTimer -= Time.deltaTime;
        }
        else
        {
            turnTimer = Random.Range(0, 5);
            direction = GetDirectionTowardsTarget();
        }

        transform.rotation = Quaternion.LookRotation(Vector3.Lerp(transform.forward, direction, turnSpeed * Time.deltaTime));
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    void SteerOnRails()
    {
        Vector3 heading = GetDirectionTowardsTarget();
        if (Vector3.Dot(Camera.main.transform.forward, heading) > 0)
        {
            direction = SteerTowardsTarget();
            transform.rotation = Quaternion.LookRotation(direction);
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0 && !effect.enabled)
            {
                if (Vector3.Dot(GetDirectionTowardsTarget(), transform.forward) > shootThreshold)
                {
                    ShootAtPlayer();
                }
            }
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void ShootAtPlayer()
    {
        var obj = GameManager.Get().objectPool.Spawn("bullet", transform.position);
        var b = obj.GetComponent<Bullet>();
        b.explosive = false;
        b.direction = GetDirectionTowardsTarget();
        b.owner = gameObject;
        b.damage = attackPower;
        b.trail.material.SetColor("_Color", Color.white * 2);

        shootTimer = Random.Range(0.1f, fireRate);
    }
    
    Vector3 GetDirectionTowardsTarget()
    {
        return (GameManager.Get().playerShip.transform.position - transform.position).normalized;
    }
    
    Vector3 SteerTowardsTarget()
    {
        return Vector3.Lerp(transform.forward, GetDirectionTowardsTarget(), turnSpeed * Time.deltaTime).normalized;
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
            return Vector3.Lerp(transform.forward, (transform.position - steer).normalized, turnSpeed * Time.deltaTime);
        }
        return transform.forward;
    }
}
