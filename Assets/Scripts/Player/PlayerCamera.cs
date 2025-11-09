using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public ParticleSystem boostEffect;
    public PlayerShip player;
    public Transform onRailsFollowTarget;

    [SerializeField] float offsetY = 3;
    public float maxDistanceFromPlayer = 10;
    public float cameraLerpSpeed = 10;

    float t = 0;

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
        Vector3 camPos = player.transform.position + (player.transform.up * offsetY) - (transform.forward * maxDistanceFromPlayer);


        if(player.strafeMode)
        {
            Cursor.lockState = CursorLockMode.Locked;
            t += Time.deltaTime;
            t = Mathf.Clamp01(t);
            transform.position = Vector3.Lerp(transform.position, camPos, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, t);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
            t = 0;
            transform.position = Vector3.Lerp(transform.position, camPos, cameraLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, cameraLerpSpeed * Time.deltaTime);
        }

    }

    void FollowOnRailsTarget()
    {
        Quaternion targetRot = Quaternion.Euler(onRailsFollowTarget.localEulerAngles);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, cameraLerpSpeed * Time.deltaTime);
        Vector3 camPos = onRailsFollowTarget.position + (onRailsFollowTarget.up * 3) - (transform.forward * maxDistanceFromPlayer);
        transform.position = Vector3.Lerp(transform.position, camPos, cameraLerpSpeed * Time.deltaTime);
    }
}
