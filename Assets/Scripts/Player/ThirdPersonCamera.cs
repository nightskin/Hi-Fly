using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float cameraDistance = 10;
    public Vector2 cameraOffset;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float lerpSpeed = 20;
    [SerializeField] PlayerShip player;

    Vector2 cameraRot;

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
            cameraRot += InputManager.shipInput.actions.Move.ReadValue<Vector2>() * rotationSpeed * Time.deltaTime;
            cameraRot.y = Mathf.Clamp(cameraRot.y, -90, 90);
            Vector3 finalRot = new Vector3(cameraRot.y, cameraRot.x, 0);
            Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
            transform.localEulerAngles = finalRot;
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
        }
    }

}
