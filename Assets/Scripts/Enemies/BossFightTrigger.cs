using UnityEngine;

public class BossFightTrigger : MonoBehaviour
{
    [SerializeField] GameObject boss;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            boss.SetActive(true);
        }      
    }
}
