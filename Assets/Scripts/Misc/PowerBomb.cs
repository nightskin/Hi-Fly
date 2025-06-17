using UnityEngine;

public class PowerBomb : MonoBehaviour
{
    [SerializeField][Min(0.1f)] float maxScale = 20;
    [SerializeField][Min(1)] float shrinkRate = 5;
    [SerializeField][Min(0)] float timeBeforeShrink = 1;
    [SerializeField][Min(0)] float damage = 30;
    float timer;

    void OnEnable()
    {
        timer = 0;
        transform.localScale = Vector3.one * maxScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= timeBeforeShrink)
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
