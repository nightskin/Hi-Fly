using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public PlayerShip player;
    public Transform onRailsFollowTarget;

    public float offset = 3;
    public float distanceFromPlayer = 10;
    public float lerpSpeed = 10;

    void Update()
    {
        if (!GameManager.Get().gamePaused)
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
        if(player.strafeMode)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0, offset, -distanceFromPlayer), lerpSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 targetPosition = player.transform.position + (player.transform.up * offset) - (player.transform.forward * distanceFromPlayer);
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, player.transform.rotation, lerpSpeed * Time.deltaTime);
        }
    }

    void FollowOnRailsTarget()
    {
        Vector3 camPos = onRailsFollowTarget.position + (onRailsFollowTarget.up * 3) - (transform.forward * distanceFromPlayer);
        transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, onRailsFollowTarget.rotation, lerpSpeed * Time.deltaTime);
    }
}
