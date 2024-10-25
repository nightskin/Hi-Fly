using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float cameraDistance = 10;
    public Vector2 cameraOffset;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float lerpSpeed = 10;
    [SerializeField] PlayerShip player;

    Vector2 cameraRot;
    float zRot = 0;

    void Start()
    {
        if(!boostEffect) boostEffect = transform.GetChild(0).GetComponent<ParticleSystem>();
        if(!player) player = GameObject.FindWithTag("Player").GetComponent<PlayerShip>();
    }

    void OnEnable()
    {
        cameraRot = Vector2.zero;
    }

    void Update()
    {
        if(player.health.IsAlive())
        {
            cameraRot += InputManager.input.Player.Steer.ReadValue<Vector2>() * rotationSpeed * Time.deltaTime;

            if(Vector3.Dot(player.transform.up, transform.up) < 0)
            {
                zRot += 180;
            }

            Vector3 camRot = new Vector3(cameraRot.y, cameraRot.x, zRot);
            Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(camRot), lerpSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
        }
    }

}
