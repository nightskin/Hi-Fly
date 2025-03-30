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
        Vector2 steerInput = InputManager.input.Player.Steer.ReadValue<Vector2>();
        cameraRot += new Vector3(steerInput.y, steerInput.x) * rotationSpeed * Time.deltaTime;
        cameraRot.x = ConstrainAngle(cameraRot.x);
        cameraRot.y = ConstrainAngle(cameraRot.y);

        Vector3 camPos = player.transform.position + new Vector3(cameraOffset.x, cameraOffset.y, 0) - transform.forward * cameraDistance;
        
        transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(cameraRot), lerpSpeed * Time.deltaTime);


    }

    float ConstrainAngle(float v)
    {
        v -= 360 * Mathf.Floor((v + 180.0f) * (1.0f / 360.0f));
        return v;
    }

}
