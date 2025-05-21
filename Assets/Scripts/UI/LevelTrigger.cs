using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    bool allowTrigger;

    void Start()
    {
        allowTrigger = true;    
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" && allowTrigger)
        {
            HubMenu.levelName = transform.parent.name;
            FindAnyObjectByType<HubMenu>(FindObjectsInactive.Include).OpenLevelMenu();
            allowTrigger = false;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.tag == "Player")
        {
            allowTrigger = true;
        }
    }
}
