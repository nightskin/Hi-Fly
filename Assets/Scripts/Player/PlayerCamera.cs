using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float cameraDistance = 10;
    public Vector2 cameraOffset;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float lerpSpeed = 10;
    [SerializeField] PlayerShip player;

    [SerializeField] Vector3 cameraRot = Vector3.zero;
    bool invertY = false;

    void Start()
    {
        if(!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        if(!player) player = GameObject.FindWithTag("Player").GetComponent<PlayerShip>();
        InputManager.input.Player.Steer.performed += Check_SteerY;
    }

    void Check_SteerY(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(context.ReadValue<Vector2>().y > 0.1f || context.ReadValue<Vector2>().y < -0.1f)
        {
            if(cameraRot.z != 0)
            {
                invertY = true;
            }
            else
            {
                invertY = false;
            }
        }
    }

    void Update()
    {
        if(player.health.IsAlive())
        {
            CameraMovement();
        }
    }

    void OnDestroy()
    {
        InputManager.input.Player.Steer.performed -= Check_SteerY;
    }

    void CameraMovement()
    {
        if(GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE)
        {
            Vector2 steerInput = InputManager.input.Player.Steer.ReadValue<Vector2>();
            if(invertY)
            {
                cameraRot += new Vector3(-steerInput.y, steerInput.x) * rotationSpeed * Time.deltaTime;
            }
            else
            {
                cameraRot += new Vector3(steerInput.y, steerInput.x) * rotationSpeed * Time.deltaTime;
            }

            cameraRot.x = ConstrainAngle(cameraRot.x);
            cameraRot.y = ConstrainAngle(cameraRot.y);

            if(cameraRot.x > 90 || cameraRot.x < -90)
            {
                cameraRot.z = Mathf.Lerp(cameraRot.z, 180, lerpSpeed * Time.deltaTime);
            }
            else
            {
                cameraRot.z = Mathf.Lerp(cameraRot.z, 0, lerpSpeed * Time.deltaTime);
            }

            Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(cameraRot), lerpSpeed * Time.deltaTime);

        }
        else if(GameManager.playerMode == GameManager.PlayerMode.STRAFE_MODE)
        {
            Vector2 steerInput = InputManager.input.Player.Aim.ReadValue<Vector2>();
            cameraRot += new Vector3(-steerInput.y, steerInput.x) * rotationSpeed * Time.deltaTime;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -90, 90);

            Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(cameraRot), lerpSpeed * Time.deltaTime);
        }
        else if(GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            
        }
    }

    float ConstrainAngle(float v)
    {
        v -= 360 * Mathf.Floor((v + 180.0f) * (1.0f / 360.0f));
        return v;
    }

}
