using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCamera : MonoBehaviour
{
    public float rotationSpeed = 100;
    public float cameraDistance = 10;
    public ParticleSystem boostEffect;

    [SerializeField][Min(2)] float lerpSpeed = 10;
    [SerializeField] Vector3 defaultOffset = new Vector3(0, 3, 0);
    public Transform onRailsOffset;
    [SerializeField] PlayerShip player;

    public Vector3 cameraRot = Vector3.zero;
    int currentPathIndex = 0;

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

            Vector3 camPos = (player.transform.position + defaultOffset) - (transform.forward * cameraDistance);
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(cameraRot), lerpSpeed * Time.deltaTime);

        }
        else if(GameManager.playerMode == GameManager.PlayerMode.STRAFE_MODE)
        {
            Vector2 lookInput = InputManager.input.Player.Aim.ReadValue<Vector2>();
            if(GameManager.invertLookY) cameraRot += new Vector3(lookInput.y, lookInput.x) * rotationSpeed * Time.deltaTime;
            else cameraRot += new Vector3(-lookInput.y, lookInput.x) * rotationSpeed * Time.deltaTime;
            cameraRot.x = Mathf.Clamp(cameraRot.x, -90, 90);

            Vector3 camPos = (player.transform.position + defaultOffset) - transform.forward * cameraDistance;
            transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(cameraRot);
        }
        else if(GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            onRailsOffset.position = Vector3.MoveTowards(onRailsOffset.position, GameManager.playerPath[currentPathIndex], player.speed * Time.deltaTime);
            onRailsOffset.rotation = Quaternion.LookRotation((GameManager.playerPath[currentPathIndex] - onRailsOffset.position).normalized);

            transform.position = Vector3.Lerp(transform.position, (onRailsOffset.position + defaultOffset) - (transform.forward * cameraDistance), lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, onRailsOffset.rotation, lerpSpeed * Time.deltaTime);


            if (Vector3.Distance(onRailsOffset.position, GameManager.playerPath[currentPathIndex]) < 15.0f)
            {
                if (currentPathIndex < GameManager.playerPath.Count - 1)
                {
                    currentPathIndex++;
                }
                else
                {
                    cameraRot = new Vector3(player.transform.localEulerAngles.x, player.transform.localEulerAngles.y, 0);
                    GameManager.playerMode = GameManager.PlayerMode.STANDARD_MODE;
                    Debug.Log(GameManager.playerMode.ToString());
                }
            }


        }
    }

}
