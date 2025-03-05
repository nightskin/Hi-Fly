using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float cameraDistance = 10;
    public Vector2 cameraOffset;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float lerpSpeed = 10;
    [SerializeField] PlayerShip player;

    Vector2 cameraRot = Vector2.zero;

    void Start()
    {
        if(!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        if(!player) player = GameObject.FindWithTag("Player").GetComponent<PlayerShip>();

    }

    void Update()
    {
        if(player.health.IsAlive())
        {
            LookAround();
        }
    }

    void LookAround()
    {
        if (player.strafeMode)
        {
            Vector2 look = InputManager.input.Player.Aim.ReadValue<Vector2>();
            cameraRot += new Vector2(look.x, -look.y) * rotationSpeed * Time.deltaTime;
        }
        else
        {
            cameraRot += InputManager.input.Player.Steer.ReadValue<Vector2>() * rotationSpeed * Time.deltaTime;
        }
        
        cameraRot.y = Mathf.Clamp(cameraRot.y, -90, 90);

        Vector3 camRot = new Vector3(cameraRot.y, cameraRot.x, 0);
        Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
        
        transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(camRot), lerpSpeed * Time.deltaTime);

    }

}
