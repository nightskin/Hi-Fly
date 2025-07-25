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
        if(GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE)
        {
            Quaternion targetRot = Quaternion.Euler(player.transform.localEulerAngles.x, player.transform.localEulerAngles.y, player.transform.localEulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            Vector3 camPos = player.transform.position + (player.transform.up * 3) - (transform.forward * distance);
            transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
        }
        else if(GameManager.playerMode == GameManager.PlayerMode.HOVER_MODE)
        {
            Vector3 camPos = followTarget.position + (followTarget.transform.up * 3) - (transform.forward * distance);
            transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.transform.forward), camSpeed * Time.deltaTime);
        }
    }

}
