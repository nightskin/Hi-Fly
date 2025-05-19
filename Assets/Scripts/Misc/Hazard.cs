using UnityEngine;

public class Hazard : MonoBehaviour
{
    [SerializeField] bool continousDamage;
    [SerializeField] [Min(0)] float damage = 1;

    Collider collider;
    BoxCollider box;
    SphereCollider sphere;

    void Start()
    {
        collider = GetComponent<Collider>();

        if(collider.GetType() == typeof(BoxCollider))
        {
            box = (BoxCollider)collider;
        }
        else if(collider.GetType() == typeof (SphereCollider))
        {
            sphere = (SphereCollider)collider;
        }
        else
        {
            Debug.Log("Hazard Needs A Collider");
        }
    }

    void FixedUpdate()
    {
        if(continousDamage && collider)
        {
            CheckCollisions();
        }
    }

    void CheckCollisions()
    {
        if(box)
        {
            Collider[] hits = Physics.OverlapBox(box.center, box.size, transform.rotation);
            if(hits.Length > 0)
            {
                foreach(Collider hit in hits)
                {
                    if(hit.tag == "Player")
                    {
                        HealthSystem health = hit.GetComponent<HealthSystem>();
                        if(health) health.TakeDamage(damage);

                        if(health.IsDead())
                        {
                            GameManager.gameOver = true;
                        }

                    }
                    else if(hit.tag == "Enemy")
                    {
                        HealthSystem health = hit.GetComponent<HealthSystem>();
                        if(health) health.TakeDamage(damage);


                    }
                }
            }
        }
        else if(sphere)
        {
            Collider[] hits = Physics.OverlapSphere(sphere.center, sphere.radius);
            if (hits.Length > 0)
            {
                foreach (Collider hit in hits)
                {
                    if (hit.tag == "Player")
                    {
                        HealthSystem health = hit.GetComponent<HealthSystem>();
                        if (health) health.TakeDamage(damage);

                        if (health.IsDead())
                        {
                            GameManager.gameOver = true;
                        }
                    }
                    else if (hit.tag == "Enemy")
                    {
                        HealthSystem health = hit.GetComponent<HealthSystem>();
                        if (health) health.TakeDamage(damage);
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider hit)
    {
        if(!continousDamage)
        {
            if (hit.tag == "Player")
            {
                HealthSystem health = hit.GetComponent<HealthSystem>();
                if (health) health.TakeDamage(damage);

                if (health.IsDead())
                {
                    GameManager.gameOver = true;
                }
            }
            else if (hit.tag == "Enemy")
            {
                HealthSystem health = hit.GetComponent<HealthSystem>();
                if (health) health.TakeDamage(damage);
            }
        }
    }

    void OnCollisionEnter(Collision hit)
    {
        if (!continousDamage) 
        {
            if (hit.transform.tag == "Player")
            {
                HealthSystem health = hit.transform.GetComponent<HealthSystem>();
                if (health) health.TakeDamage(damage);

                if (health.IsDead())
                {
                    GameManager.gameOver = true;
                }
            }
            else if (hit.transform.tag == "Enemy")
            {
                HealthSystem health = hit.transform.GetComponent<HealthSystem>();
                if (health) health.TakeDamage(damage);
            }
        }
    }
}
