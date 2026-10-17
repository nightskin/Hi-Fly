using UnityEngine;
using System.Collections;

public class EnemyShip : MonoBehaviour
{
    [SerializeField] Transform bulletSpawn;
    [SerializeField] EnemyDissolveEffect effect;
    [SerializeField] HealthSystem health;
    [SerializeField][Range(0, 1)] float shootThreshold = 0.75f;
    [SerializeField] float separationRadius = 5;

    [SerializeField] float turnRate = 10;

    [SerializeField] float fireRate = 1;
    [SerializeField] float moveSpeed = 75;

    Vector3 direction;
    float shootTimer = 0;
    float steerTimer;
    float pauseTimer = 5;

    void OnEnable()
    {
        steerTimer = Random.Range(2.5f,10);
        pauseTimer = Random.Range(2.5f,10);
        effect.enabled = true;
        health = GetComponent<HealthSystem>();
        health.Heal(health.MaxHP());
        //Set colors
        GetComponent<MeshRenderer>().materials[0].SetColor("_Color", Util.RandomColor()); 
        GetComponent<MeshRenderer>().materials[2].SetColor("_Color", Util.RandomColor()); 
    }
    
    void Update()
    {
        if (GameManager.Get().state == GameManager.GameState.GAME_PLAY)
        {
            if (health.IsAlive())
            {
                if(steerTimer > 0)
                {
                    steerTimer -= Time.deltaTime;
                    Steer();
                }
                else
                {
                    if(pauseTimer > 0)
                    {
                        pauseTimer -= Time.deltaTime;
                    }
                    else
                    {
                        pauseTimer = Random.Range(2.5f,10);
                        steerTimer = Random.Range(2.5f,10);
                    }
                }


                transform.rotation = Quaternion.LookRotation(direction);
                transform.position += transform.forward * moveSpeed * Time.deltaTime;

                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0 && !effect.enabled)
                {
                    if (Vector3.Dot(GetDirectionTowardsTarget(), transform.forward) > shootThreshold)
                    {
                        ShootAtStuff();
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
            playerHealth.TakeDamage(2);
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
        GameManager.Get().objectPool.Spawn("explosion", transform.position);
        gameObject.SetActive(false);
    }

    void Steer()
    {
        direction = Seperation() + SteerTowardsTarget(); 
    }

    void ShootAtStuff()
    {
        if(Physics.Linecast(transform.position, GameManager.Get().playerObject.transform.position,out RaycastHit hit))
        {
            if(hit.transform.tag == "Player")
            {
                var obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                var b = obj.GetComponent<Bullet>();
                b.direction = (hit.transform.position - bulletSpawn.position).normalized;
                b.owner = gameObject;
                b.isPowerBomb = false;
            }
            else if(hit.transform.tag == "Destructible")
            {
                var obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                var b = obj.GetComponent<Bullet>();
                b.direction = (hit.point - bulletSpawn.position).normalized;
                b.owner = gameObject;
                b.isPowerBomb = false;
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 steer = Vector3.zero;
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
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
            steer /= colliders.Length;
            return Vector3.Lerp(transform.forward, (transform.position - steer).normalized, turnRate * Time.deltaTime).normalized;
        }
        return steer;
    }
    

}
