using UnityEngine;

public class PlayerCollisions : MonoBehaviour
{
    [SerializeField] PlayerShip playerShip;
    [SerializeField] HealthSystem health;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip crashSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Surface" || other.tag == "Destructible")
        {
            if(!playerShip.strafeMode)
            {
                health.TakeDamage(10);
                audioSource.PlayOneShot(crashSound);
                if (health.IsDead())
                {
                    GameObject.Find("ObjectPool").GetComponent<ObjectPoolManager>().Spawn("explosion", transform.position);
                    GameManager.Get().playerShip.gameObject.SetActive(false);
                    GameManager.Get().gameOver = true;
                }
            }

        }
    }

}
