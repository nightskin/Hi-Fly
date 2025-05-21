using UnityEngine;

public class LevelTransitionScene : MonoBehaviour
{
    bool activated;

    [SerializeField] Transform ship;
    [SerializeField] Transform destination;

    [SerializeField] GameObject[] objects;

    void Awake()
    {
        GameManager.gamePaused = false;
        activated = false;
        if (objects.Length > 0)
        {
            foreach(GameObject obj in objects) 
            {
                if(obj.name == HubMenu.levelName)
                {
                    Instantiate(obj, transform.position, Quaternion.identity, transform);
                    return;
                }
            }
        }
    }

    void Update()
    {
        if(Vector3.Distance(ship.position, destination.position) < 1)
        {
            if(!activated)
            {
                StartCoroutine(SceneLoader.instance.Load(HubMenu.levelName));
                activated = true;
            }
        }
    }

}
