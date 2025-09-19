using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] Sprite[] visuals;

    Transform player;
    [SerializeField] LayerMask playerLayer;

    public enum Type
    {
        HEAL,
        POWER_BOMB,
        POWER_BEAM,
        RAPID_FIRE,
    }
    public Type type;

    void Start()
    {
        player = GameManager.Get().playerShip.transform.Find("PlayerMesh");
    }

    void OnEnable()
    {
        type = (Type)Random.Range(0, 3);
        renderer.sprite = visuals[(int)type];
    }

    void FixedUpdate()
    {
        if (Physics.CheckSphere(transform.position, 1.25f, playerLayer))
        {
            ActivateEffect();
        }
    }

    public void ActivateEffect()
    {
        if (type == Type.HEAL)
        {
            player.GetComponent<HealthSystem>().Heal(30);
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

        }
        gameObject.SetActive(false);
    }

}
