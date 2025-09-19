using UnityEngine;

public class OutDoor_To_Indoor_Trigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameObject spawner = GameObject.Find("OutdoorEnemySpawner");
            if (spawner) spawner.SetActive(false);
        }
    }
}
