using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public PlayerShip player;
    public float lerpSpeed = 20;

    Vector3 offset;

    void Start()
    {
        offset = transform.localPosition;
    }

    void Update()
    {
        if (!GameManager.Get().gamePaused)
        {
            UpdateCamera();
        }
    }
    
    void UpdateCamera()
    {
        Vector3 targetPosition = player.transform.position + (player.transform.up * offset.y) + (player.transform.forward * offset.z);
        Vector3 currentPosition = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        transform.position = currentPosition;
        transform.rotation = Quaternion.Lerp(transform.rotation, player.transform.rotation, lerpSpeed * Time.deltaTime);
    }

}
