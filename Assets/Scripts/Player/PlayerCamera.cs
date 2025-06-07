using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float distance = 10;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float camSpeed = 10;

    [SerializeField] PlayerShip player;

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
        if(GameManager.playerMode == GameManager.PlayerMode.All_RANGE_MODE)
        {
            float x = InputManager.input.Player.Steer.ReadValue<Vector2>().x;
            float y = InputManager.input.Player.Steer.ReadValue<Vector2>().y;
            float z = InputManager.input.Player.SteerZ.ReadValue<float>();


            Vector3 camPos = player.transform.position + (player.transform.up * 3) - (transform.forward * distance);
            transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
            transform.rotation *= Quaternion.AngleAxis(x * rotationSpeed * Time.deltaTime, Vector3.up);
            transform.rotation *= Quaternion.AngleAxis(y * rotationSpeed * Time.deltaTime, Vector3.right);
            transform.rotation *= Quaternion.AngleAxis(z * rotationSpeed * Time.deltaTime, Vector3.forward);

        }
        else if(GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            Vector3 camPos = followTarget.position + (followTarget.transform.up * 3) - (transform.forward * distance);
            transform.position = Vector3.Lerp(transform.position, camPos, camSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(player.transform.forward), camSpeed * Time.deltaTime);
        }
    }

}
