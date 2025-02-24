using UnityEngine;

public class PlanetTrigger : MonoBehaviour
{
    [SerializeField] float triggerSize = 100;
    SphereCollider collider;
    Planet planet;

    void Start()
    {
        collider = GetComponent<SphereCollider>();
        planet = transform.parent.GetComponent<Planet>();
        collider.radius = (planet.radius + triggerSize);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            HubWorld.levelId = transform.parent.name;
            HubWorld.OpenPlanetMenu();
        }
    }
}
