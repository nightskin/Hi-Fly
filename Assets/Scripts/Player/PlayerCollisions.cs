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
            if(!playerShip.thrusting)
            {
                health.TakeDamage(10);
                audioSource.PlayOneShot(crashSound);
                if (health.IsDead())
                {
                    GameObject.Find("ObjectPool").GetComponent<ObjectPoolManager>().Spawn("explosion", transform.position);
                    GameManager.Get().playerObject.gameObject.SetActive(false);
                    GameManager.Get().gameOver = true;
                }
            }

        }
        else if (other.tag == "Bounds")
        {
           playerShip.Teleport(transform.position * -1);
        }
    }

}
