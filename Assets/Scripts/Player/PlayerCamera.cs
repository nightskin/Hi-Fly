using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float distance = 10;
    public ParticleSystem boostEffect;

    [HideInInspector] public float followSpeed;
    public float maxFollowSpeed = 40;
    public float baseFollowSpeed = 10;

    public PlayerShip player;
    public Transform onRailsFollowTarget;

    void Start()
    {
        if(player.strafeMode)
        {
            followSpeed = maxFollowSpeed;
        }
        else
        {
            followSpeed = baseFollowSpeed;
        }

        if (!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (player.health.IsAlive() && !GameManager.Get().gamePaused)
        {
            if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ON_RAILS)
            {
                FollowOnRailsTarget();
            }
            else
            {
                FollowShip();
            }
        }
    }

    void FollowShip()
    {
        Quaternion targetRot = Quaternion.Euler(player.transform.localEulerAngles);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, followSpeed * Time.deltaTime);
        Vector3 camPos = player.transform.position + (player.transform.up * 3) - (transform.forward * distance);
        transform.position = Vector3.Lerp(transform.position, camPos, followSpeed * Time.deltaTime);
    }

    void FollowOnRailsTarget()
    {
        Quaternion targetRot = Quaternion.Euler(onRailsFollowTarget.localEulerAngles);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, followSpeed * Time.deltaTime);
        Vector3 camPos = onRailsFollowTarget.position + (onRailsFollowTarget.up * 3) - (transform.forward * distance);
        transform.position = Vector3.Lerp(transform.position, camPos, followSpeed * Time.deltaTime);
    }
}
