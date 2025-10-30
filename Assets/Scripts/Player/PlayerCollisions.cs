using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] HealthSystem health;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip crashSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Surface" || other.tag == "Destructible")
        {
            health.TakeDamage(10);
            audioSource.PlayOneShot(crashSound);
            if (health.IsDead())
            {
                GameObject.Find("ObjectPool").GetComponent<ObjectPoolManager>().SpawnFromObjectPool("explosion", transform.position);
                GameManager.Get().playerShip.gameObject.SetActive(false);
                GameManager.Get().gameOver = true;
            }
        }
    }

}
