using UnityEngine;
using System.Collections.Generic;

public class EnemyShip : MonoBehaviour
{
    [SerializeField] Transform bulletSpawn;
    [SerializeField] EnemyDissolveEffect effect;
    [SerializeField] HealthSystem health;
    [SerializeField][Range(0, 1)] float shootThreshold = 0.75f;
    [SerializeField] float perceptionRadius = 20;
    [SerializeField] int attackPower = 10;
    [SerializeField] float turnRate = 10;

    [SerializeField] float fireRate = 1;
    [SerializeField] float moveSpeed = 75;


    bool isSmart;
    Vector3 direction;
    float shootTimer = 0;
    float turnTimer;

    void OnEnable()
    {
        isSmart = Util.RandomBool();
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
                if (isSmart) Steer_Smart();
                else Steer_Dumb();

                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0 && !effect.enabled)
                {
                    if (Vector3.Dot(GetDirectionTowardsTarget(), transform.forward) > shootThreshold)
                    {
                        ShootShit();
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

    public void Die()
    {   
        EnemyWaveManager.Get().enemiesInWave--;
        gameObject.SetActive(false);
    }

    void Steer_Smart()
    {
        direction = SteerTowardsTarget() + Seperation();
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

        transform.rotation = Quaternion.LookRotation(Vector3.Lerp(transform.forward, direction, turnRate * Time.deltaTime));
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
    
    void ShootShit()
    {
        if(Physics.Linecast(transform.position, GameManager.Get().playerObject.transform.position,out RaycastHit hit))
        {
            if(hit.transform.tag == "Player")
            {
                var obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                var b = obj.GetComponent<Bullet>();
                b.direction = (hit.transform.position - bulletSpawn.position).normalized;
                b.owner = gameObject;
                b.damage = attackPower;

            }
            else if(hit.transform.tag == "Destructible")
            {
                var obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                var b = obj.GetComponent<Bullet>();
                b.direction = (hit.point - bulletSpawn.position).normalized;
                b.owner = gameObject;
                b.damage = attackPower;
            }
            shootTimer = Random.Range(0, fireRate);
        }
    }
    
    Vector3 GetDirectionTowardsTarget()
    {
        return (GameManager.Get().playerObject.transform.position - transform.position).normalized;
    }
    
    Vector3 SteerTowardsTarget()
    {
        return Vector3.Lerp(transform.forward, GetDirectionTowardsTarget(), turnRate * Time.deltaTime).normalized;
    }

    Vector3 Seperation()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, perceptionRadius);
        Vector3 steer = Vector3.zero;
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                if (Vector3.Dot(transform.forward, collider.transform.forward) >= shootThreshold)
                {
                    if(Physics.Linecast(transform.position, collider.transform.position, out RaycastHit hit))
                    {
                        steer += hit.point;
                    }
                    else
                    {
                        steer += collider.transform.position;
                    }
                }
            }
            steer /= colliders.Length;
            return Vector3.Lerp(transform.forward, (transform.position - steer).normalized, turnRate * Time.deltaTime).normalized;
        }
        return steer;
    }

}
