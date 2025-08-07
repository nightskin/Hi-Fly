using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 10;
    public float distance = 10;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float camSpeed = 10;

    [SerializeField] PlayerShip player;

    public Transform followTarget;

    void Start()
    {
        if(!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if(player.health.IsAlive() && !GameManager.gamePaused)
        {
            CameraMovement();
        }
    }

    void CameraMovement()
    {
        Quaternion targetRot = Quaternion.Euler(player.transform.localEulerAngles.x, player.transform.localEulerAngles.y, player.transform.localEulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        Vector3 camPos = player.transform.position + (player.transform.up * 3) - (transform.forward * distance);
        transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
    }

}
