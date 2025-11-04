using UnityEngine;

public class PowerBomb : MonoBehaviour
{
    public float blastRadius = 20;
    public int damage = 30;


    float shrinkRate = 5;
    float timer;


    void OnEnable()
    {
        timer = 0;
        transform.localScale = Vector3.one * blastRadius;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 1)
        {
            transform.localScale -= Vector3.one * shrinkRate * Time.deltaTime;
            if(transform.localScale.x <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "Enemy")
        {
            HealthSystem health = hit.GetComponent<HealthSystem>();
            if (health) health.TakeDamage(damage);
        }
        
    }

}
