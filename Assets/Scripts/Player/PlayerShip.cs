using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerShip : MonoBehaviour
{
    //Necessary Components
    public HealthSystem health;
    [SerializeField] PlayerCamera camera;
    [SerializeField] TrailRenderer[] trails;
    [SerializeField] TrailRenderer thruster;
    [SerializeField] Transform mesh;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] CharacterController controller;
    
    Color thrustColor = Color.cyan;
    
    [SerializeField][Min(1)] float turnSpeed = 5;
    [SerializeField] float baseSpeed = 10;
    [SerializeField] float boostSpeed = 50;
    [SerializeField] float acceleration = 10;

    bool evading = false;
    int evadeDirection = 0;
    float evadeTimer;
    float evadeSpeed = 360 * 5;

    float targetSpeed;
    public float speed;

    //For Shooting
    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;
    bool aimingViaGamepad = false;
    Vector2 reticlePosition;
    RaycastHit lockOn;

    public static float fireRate = 1f;
    public static float bulletPower = 10;
    public static float lazerPower = 10;


    ObjectPool objectPool;

    Lazer lazer = null;
    float shootTimer = 0;


    void Start()
    {
        Cursor.visible = false;
        mesh.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameManager.playerBodyColor);
        mesh.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameManager.playerStripeColor);


        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
        if (trails.Length == 0) trails = GetComponentsInChildren<TrailRenderer>();
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<PlayerCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;

        InputManager.input.Player.Fire1.performed += Fire1_performed;
        InputManager.input.Player.Fire2.performed += Fire2_performed;
        InputManager.input.Player.Fire2.canceled += Fire2_canceled;
        InputManager.input.Player.Aim.performed += Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed += Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed += CenterCrosshair_performed;
        InputManager.input.Player.Brake.performed += Brake_performed;
        InputManager.input.Player.Boost.performed += Boost_performed;
        InputManager.input.Player.Boost.canceled += Boost_canceled;
        InputManager.input.Player.Evade.performed += Evade_performed;
    }
    
    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(reticle.rectTransform.position);
        if (Physics.SphereCast(ray, 4, out lockOn, Camera.main.farClipPlane, lockOnLayer))
        {
            reticle.color = Color.red;
        }
        else
        {
            reticle.color = Color.white;
        }
    }

    void Update()
    {
        if (health.IsAlive() && !GameManager.gamePaused)
        {
            Controls();
            
            //Shooting
            if (InputManager.input.Player.Fire1.IsPressed())
            {
                if (shootTimer > 0)
                {
                    shootTimer -= Time.deltaTime;
                }
                else
                {
                    FireBullet();
                    shootTimer = fireRate;
                }
            }
            else if (InputManager.input.Player.Fire2.IsPressed())
            {
                if (lazer)
                {
                    if (lockOn.collider)
                    {
                        lazer.direction = (lockOn.point - lazer.origin).normalized;
                    }
                    else
                    {
                        Ray ray = Camera.main.ScreenPointToRay(reticle.rectTransform.position);
                        if (Physics.Raycast(ray, out RaycastHit hit, Camera.main.farClipPlane))
                        {
                            if (hit.transform.tag != "Player")
                            {
                                lazer.direction = (hit.point - lazer.origin).normalized;
                            }
                        }
                        else
                        {
                            lazer.direction = ray.direction;
                        }
                    }
                }
            }
        }
    }

    void OnDestroy()
    {
        InputManager.input.Player.Fire1.performed -= Fire1_performed;
        InputManager.input.Player.Fire2.performed -= Fire2_performed;
        InputManager.input.Player.Fire2.canceled -= Fire2_canceled;
        InputManager.input.Player.Aim.performed -= Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed -= Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed -= CenterCrosshair_performed;
        InputManager.input.Player.Brake.performed -= Brake_performed;
        InputManager.input.Player.Boost.performed -= Boost_performed;
        InputManager.input.Player.Boost.canceled -= Boost_canceled;
        InputManager.input.Player.Evade.performed -= Evade_performed;
    }

    void OnTriggerEnter(Collider other)
    {   
        if(other.tag == "Bounds")
        {
            Teleport(transform.position * -1);
        }
    }

    private void Boost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.gamePaused && !GameManager.gameOver)
        {
            camera.boostEffect.Play();
            thrustColor = Color.red;
            targetSpeed = boostSpeed;
            foreach (TrailRenderer trail in trails)
            {
                trail.emitting = true;
            }
        }
    }

    private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        camera.boostEffect.Stop();
        thrustColor = Color.cyan;
        targetSpeed = baseSpeed;
    }
    
    private void Fire1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        shootTimer = 0;
    }

    private void Fire2_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (GameManager.currentPowerUp == GameManager.PlayerPowerUp.POWER_BEAM)
        {
            FireLazer();
        }
        else if (GameManager.currentPowerUp == GameManager.PlayerPowerUp.POWER_BOMB)
        {
            FirePowerBomb();
        }
    }

    private void Fire2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (lazer)
        {
            lazer.GetComponent<Lazer>().DeSpawn();
            lazer = null;
        }
    }
    
    private void Brake_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.gamePaused && !GameManager.gameOver)
        {
            if(targetSpeed > 0)
            {
                camera.boostEffect.Stop();
                thrustColor = Color.cyan;
                targetSpeed = 0;
                foreach (TrailRenderer trail in trails)
                {
                    trail.emitting = false;
                }
            }
            else
            {
                camera.boostEffect.Stop();
                targetSpeed = baseSpeed;
                foreach (TrailRenderer trail in trails)
                {
                    trail.emitting = true;
                }
            }
        }
    }
    
    private void CenterCrosshair_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
    }

    private void Gamepad_Aim_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        aimingViaGamepad = true;
    }

    private void Mouse_Aim_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        aimingViaGamepad = false;
    }

    private void Evade_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!evading)
        {
            evadeTimer = 0;
            evading = true;
            if (evadeDirection == 1) evadeDirection = -1;
            else evadeDirection = 1;
        }
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        camera.transform.position = position + (transform.up * 3) - (camera.transform.forward * camera.distance);
        controller.enabled = true;
    }

    void Controls()
    {
        //Forward Movement
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        controller.Move(transform.forward * speed * Time.deltaTime);
        thruster.endColor = Color.Lerp(thruster.endColor, thrustColor, 5 * Time.deltaTime);


        //steering
        float x = InputManager.input.Player.Steer.ReadValue<Vector2>().x;
        float y = InputManager.input.Player.Steer.ReadValue<Vector2>().y;


        transform.rotation *= Quaternion.AngleAxis(x * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(y * turnSpeed * Time.deltaTime, Vector3.right);

        if (transform.localEulerAngles.x > 90 || transform.localEulerAngles.x < -90)
        {
            if (x == 0 && y == 0)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(transform.localEulerAngles.z, 0, 5 * Time.deltaTime));
            }
        }

        if (evading)
        {
            mesh.localEulerAngles += new Vector3(0, 0, evadeDirection * evadeSpeed * Time.deltaTime);
            evadeTimer += Time.deltaTime;
            if (evadeTimer > 1)
            {
                evading = false;
            }
        }
        else
        {
            mesh.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(mesh.localEulerAngles.z, x * -45, 10 * Time.deltaTime));
        }

        //Aiming
        if (aimingViaGamepad)
        {
            reticlePosition += InputManager.input.Player.Aim.ReadValue<Vector2>() * GameManager.aimSensitivy * Time.deltaTime;
            reticlePosition.x = Mathf.Clamp(reticlePosition.x, reticle.rectTransform.sizeDelta.x / 2, Screen.width - (reticle.rectTransform.sizeDelta.x / 2));
            reticlePosition.y = Mathf.Clamp(reticlePosition.y, reticle.rectTransform.sizeDelta.y / 2, Screen.height - (reticle.rectTransform.sizeDelta.y / 2));
            reticle.rectTransform.position = reticlePosition;
        }
        else
        {
            reticlePosition = Input.mousePosition;
            reticlePosition.x = Mathf.Clamp(reticlePosition.x, reticle.rectTransform.sizeDelta.x / 2, Screen.width - (reticle.rectTransform.sizeDelta.x / 2));
            reticlePosition.y = Mathf.Clamp(reticlePosition.y, reticle.rectTransform.sizeDelta.y / 2, Screen.height - (reticle.rectTransform.sizeDelta.y / 2));
            reticle.rectTransform.position = reticlePosition;
        }
    }

    void FireBullet()
    {
        //Initialize Bullet
        GameObject obj = objectPool.Spawn("bullet", bulletSpawn.position);
        if (obj != null)
        {
            Bullet b = obj.GetComponent<Bullet>();
            //Set Needed Variables
            b.owner = mesh.gameObject;
        
            if (lockOn.collider)
            {
                b.homingTarget = lockOn.collider.transform;
            }
            else
            {
                Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(reticle.rectTransform.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform.gameObject == b.owner)
                    {
                        b.direction = ray.direction;
                    }
                    else
                    {
                        b.direction = (hit.point - bulletSpawn.position).normalized;
                    }
                }
                else
                {
                    b.direction = ray.direction;
                }
            }
        }
    }

    void FirePowerBomb()
    {
        //Initialize Bullet
        GameObject obj = objectPool.Spawn("missile", bulletSpawn.position);
        if (obj != null)
        {
            Missile m = obj.GetComponent<Missile>();
            //Set Needed Variables
            m.owner = mesh.gameObject;
        
            if (lockOn.collider)
            {
                m.homingTarget = lockOn.collider.transform;
            }
            else
            {
                Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(reticle.rectTransform.position);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.transform.gameObject == m.owner)
                    {
                        m.direction = ray.direction;
                    }
                    else
                    {
                        m.direction = (hit.point - bulletSpawn.position).normalized;
                    }
                }
                else
                {
                    m.direction = ray.direction;
                }
            }
        }
    }

    void FireLazer()
    {
        if (lazer == null)
        {
            lazer = objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
            lazer.owner = mesh.gameObject;
            lazer.damage = lazerPower;

            Ray ray = Camera.main.ScreenPointToRay(reticle.rectTransform.position);
            if (Physics.Raycast(ray, out RaycastHit hit, Camera.main.farClipPlane, lockOnLayer))
            {
                lazer.direction = (hit.point - lazer.origin).normalized;
            }
            else
            {
                lazer.direction = ray.direction;
            }
        }
    }

}
