using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Sprite[] visuals;

    public enum Type
    {
        HEALTH_SMALL,
        POWER_BOMB,
        LAZER,
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
            else if(type == Type.LAZER)
            {
                GameManager.currentPowerUp = GameManager.PlayerPowerUp.POWER_BEAM;
            }
            else if(type == Type.POWER_BOMB)
            {
                GameManager.currentPowerUp = GameManager.PlayerPowerUp.POWER_BOMB;
            }
            gameObject.SetActive(false);
        }
    }

}
