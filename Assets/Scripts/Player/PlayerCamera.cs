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
    float zRot = 0;

    void Start()
    {
        if(!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        if(!player) player = GameObject.FindWithTag("Player").GetComponent<PlayerShip>();

        if(player)
        {
            if(player.pointOfView == PlayerShip.ViewPoint.FIRST_PERSON)
            {
                transform.localPosition = Vector3.zero;
                cameraOffset = Vector2.zero;
                cameraDistance = 0;
                transform.parent = player.GetComponent<CharacterController>().transform;
            }
        }

    }

    void Update()
    {
        if(player.health.IsAlive())
        {
            if (player.pointOfView == PlayerShip.ViewPoint.THIRD_PERSON) ThirdPersonLook();
            else if(player.pointOfView == PlayerShip.ViewPoint.FIRST_PERSON) FirstPersonLook();
        }
    }

    void ThirdPersonLook()
    {
        cameraRot += InputManager.input.Player.Steer.ReadValue<Vector2>() * rotationSpeed * Time.deltaTime;

        cameraRot.y = Mathf.Clamp(cameraRot.y, -90, 90);

        Vector3 camRot = new Vector3(cameraRot.y, cameraRot.x, 0);
        Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(camRot), lerpSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
    }

    void FirstPersonLook()
    {
        cameraRot += InputManager.input.Player.FirstPersonAim.ReadValue<Vector2>() * rotationSpeed * Time.deltaTime;
        cameraRot.y = Mathf.Clamp(cameraRot.y, -90, 90);
        Vector3 camRot = new Vector3(-cameraRot.y, cameraRot.x, 0);
        transform.localRotation = Quaternion.Euler(camRot);
    }
}
