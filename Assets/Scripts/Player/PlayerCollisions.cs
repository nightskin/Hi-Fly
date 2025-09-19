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
        }
    }

}
