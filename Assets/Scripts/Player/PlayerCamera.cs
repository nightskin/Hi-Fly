using UnityEngine;
using UnityEngine.Splines;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float distance = 10;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float lerpSpeed = 10;
    [SerializeField] Vector3 offset = new Vector3(0, 3, 0);

    [SerializeField] PlayerShip player;

    Vector3 cameraRot = Vector3.zero;

    public Transform followTarget;

    void Start()
    {
        if(!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        if(!player) player = GameObject.FindWithTag("Player").GetComponent<PlayerShip>();
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
            Vector2 steerInput= InputManager.input.Player.Steer.ReadValue<Vector2>();
            if(GameManager.invertSteerY) cameraRot += new Vector3(steerInput.y, steerInput.x) * rotationSpeed * Time.deltaTime;
            else cameraRot += new Vector3(-steerInput.y, steerInput.x) * rotationSpeed * Time.deltaTime;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -90, 90);

            Vector3 camPos = (player.transform.position + offset) - (transform.forward * distance);
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(cameraRot), lerpSpeed * Time.deltaTime);

        }
        else if(GameManager.playerMode == GameManager.PlayerMode.STRAFE_MODE)
        {
            Vector2 lookInput = InputManager.input.Player.Aim.ReadValue<Vector2>();
            if(GameManager.invertLookY) cameraRot += new Vector3(lookInput.y, lookInput.x) * rotationSpeed * Time.deltaTime;
            else cameraRot += new Vector3(-lookInput.y, lookInput.x) * rotationSpeed * Time.deltaTime;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -90, 90);

            Vector3 camPos = (player.transform.position + offset) - transform.forward * distance;
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(cameraRot);
        }
        else if(GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            Vector3 camPos = (followTarget.position + offset) - (transform.forward * distance);
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.transform.forward), lerpSpeed * Time.deltaTime);
        }
    }

}
