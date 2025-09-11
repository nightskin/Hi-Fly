using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Sprite[] visuals;

    public enum Type
    {
        HEAL,
        POWER_BOMB,
        POWER_BEAM,
        RAPID_FIRE,
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
        if(Vector3.Distance(transform.position, GameManager.Get().playerShip.transform.position) < 10 || GameManager.Get().playerMode == GameManager.PlayerMode.ON_RAILS)
        {
            followPlayer = true;
        }
        if (followPlayer)
        {
            transform.position = Vector3.MoveTowards(transform.position, GameManager.Get().playerShip.transform.position, 50 * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if (type == Type.HEAL)
            {
                other.GetComponent<HealthSystem>().Heal(30);
            }
            else if (type == Type.POWER_BEAM)
            {
                GameManager.Get().ChangePowerUp(GameManager.PlayerPowerUp.POWER_BEAM);
            }
            else if (type == Type.POWER_BOMB)
            {
                GameManager.Get().ChangePowerUp(GameManager.PlayerPowerUp.POWER_BOMB);
            }
            else if (type == Type.RAPID_FIRE)
            {
                //GameManager.Get().ChangePowerUp(GameManager.)
            }
            gameObject.SetActive(false);
        }
    }

}
