using UnityEngine;
using UnityEngine.UIElements;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 10;
    public float distance = 10;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float camSpeed = 10;

    [SerializeField] PlayerShip player;

    public Transform onRailsFollowTarget;

    void Start()
    {
        if (!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (player.health.IsAlive() && !GameManager.gamePaused)
        {
            if (GameManager.playerMode == GameManager.PlayerMode.ON_RAILS)
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
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        Vector3 camPos = player.transform.position + (player.transform.up * 3) - (transform.forward * distance);
        transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
    }

    void FollowOnRailsTarget()
    {
        Quaternion targetRot = Quaternion.Euler(onRailsFollowTarget.localEulerAngles);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        Vector3 camPos = onRailsFollowTarget.position + (onRailsFollowTarget.up * 3) - (transform.forward * distance);
        transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
    }
}
