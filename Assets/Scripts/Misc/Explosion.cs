using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] bool doesDamage;
    public float blastRadius = 10;

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, blastRadius);    
    }
    
    void Update()
    {
        if(doesDamage)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius);
            if(hits.Length > 0) 
            {
                foreach(var hit in hits) 
                {
                    if(hit.tag == "Enemy")
                    {
                        HealthSystem health = hit.GetComponent<HealthSystem>();
                        if(health)
                        {
                            health.TakeDamage(30);
                        }
                    }
                    else if(hit.tag == "Player")
                    {
                        HealthSystem health = hit.GetComponent<HealthSystem>();
                        if (health)
                        {
                            health.TakeDamage(10);
                        }
                    }
                }
            }
        }

        if(!GetComponent<ParticleSystem>().isEmitting)
        {
            gameObject.SetActive(false);
        }
    }
}
