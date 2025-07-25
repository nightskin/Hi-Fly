using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Sprite[] visuals;

    public enum Type
    {
        HEALTH_SMALL,
        HEALTH_LARGE,
        HEALTH_MAX,
    }
    public Type type;
    
    bool followPlayer;

    void OnEnable()
    {
        type = (Type)Random.Range(0, 3);
        renderer.sprite = visuals[(int)type];
        followPlayer = false;
    }

    void Update()
    {
        if(Vector3.Distance(transform.position, GameManager.playerShip.transform.position) < 10)
        {
            followPlayer = true;
        }
        if(followPlayer) transform.position = Vector3.Lerp(transform.position, GameManager.playerShip.transform.position, 50 * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(type == Type.HEALTH_SMALL)
            {
                other.GetComponent<HealthSystem>().Heal(30);
            }
            else if(type == Type.HEALTH_LARGE)
            {
                other.GetComponent<HealthSystem>().Heal(50);
            }
            else if(type == Type.HEALTH_MAX)
            {
                other.GetComponent<HealthSystem>().Heal(other.GetComponent<HealthSystem>().GetMaxHealth());
            }
            gameObject.SetActive(false);
        }
    }

}
