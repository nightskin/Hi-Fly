using System.Collections.Generic;
using UnityEngine;

public class PowerBomb : MonoBehaviour
{
    public float blastRadius;
    public int damage = 30;


    float shrinkRate = 5;
    float timer;

    List<Collider> alreadyHit;

    void OnEnable()
    {
        alreadyHit = new List<Collider>();
        timer = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= 1)
        {
            blastRadius -= shrinkRate * Time.deltaTime;
            if(blastRadius <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        transform.localScale = Vector3.one * blastRadius * 2;

    }

    void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, blastRadius);
        if(hits.Length > 0)
        {
            foreach(Collider hit in hits)
            {
                HealthSystem health = hit.GetComponent<HealthSystem>();
                if (health && !alreadyHit.Contains(hit))
                {
                    health.TakeDamage(damage);
                    alreadyHit.Add(hit);
                }
            }
        }
    }
}
