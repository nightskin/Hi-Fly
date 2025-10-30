using UnityEngine;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviour
{
    public enum Weapon
    {
        NORMAL_BULLET,
        CHARGE_BOMB,
        RAVER_LAZER,
    }
    [HideInInspector] public Weapon weapon = Weapon.NORMAL_BULLET;

    //Necessary Components
    public HealthSystem health;
    public GameObject drillDasher;
    public PlayerOrbiters orbiters;
    public PlayerCamera camera;
    [SerializeField] TrailRenderer[] trails;
    [SerializeField] ParticleSystem chargeEffect;
    [SerializeField] TrailRenderer thruster;
    public Transform mesh;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] CharacterController controller;
    [SerializeField] Transform OnRailsFollowTarget;
    
    
    Vector3 offset = Vector3.one *  0.5f;
    Color thrustColor = Color.cyan;
    [SerializeField][Min(1)] float turnSpeed = 5;
    [SerializeField] float baseSpeed = 10;
    [SerializeField] float boostSpeed = 50;
    [SerializeField] float acceleration = 10;

    bool strafeMode = false;
    bool evading = false;
    int evadeDirection = 0;
    float evadeTimer;
    float evadeSpeed = 360 * 5;

    float targetSpeed;
    [HideInInspector] public float speed;

    //For Shooting
    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;
    bool aimingViaGamepad = false;
    Vector2 reticlePosition;
    RaycastHit lockOn;


    [SerializeField] Material chargeMaterial;
    bool charging = false;
    float chargeAmount = 0;
    Lazer lazer = null;
    float shootTimer = 0;

    public static float chargeSpeed = 0.5f;
    public static float fireRate = 1;
    public static int firePower = 5;

    
    
    void Start()
    {
        Cursor.visible = false;
        mesh.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        mesh.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);
        
        if (trails.Length == 0) trails = GetComponentsInChildren<TrailRenderer>();
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<PlayerCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;

        InputManager.input.Player.Fire1.performed += Fire1_performed;
        InputManager.input.Player.Fire1.canceled += Fire1_canceled;
        InputManager.input.Player.Fire2.performed += Fire2_performed;
        InputManager.input.Player.Fire2.canceled += Fire2_canceled;
        InputManager.input.Player.Aim.performed += Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed += Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed += CenterCrosshair_performed;
        InputManager.input.Player.Brake.performed += Brake_performed;
        InputManager.input.Player.Boost.performed += Boost_performed;
        InputManager.input.Player.Boost.canceled += Boost_canceled;
        InputManager.input.Player.Evade.performed += Evade_performed;

        if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ON_RAILS)
        {
            transform.parent = OnRailsFollowTarget;
        }
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
        if (health.IsAlive() && !GameManager.Get().gamePaused)
        {
            if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ALL_RANGE)
            {
                if (strafeMode) StrafeMode();
                else AllRangeMode();
            }
            else if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ON_RAILS)
            {
                OnRailsMode();
            }
            
            //Shooting
            if (InputManager.input.Player.Fire1.IsPressed())
            {
                if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.NORMAL_BULLET)
                {
                    if (shootTimer > 0)
                    {
                        shootTimer -= fireRate * Time.deltaTime;
                    }
                    else
                    {
                        FireBullet();
                        shootTimer = 1;
                    }
                }
                else if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.RAVER_LAZER)
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
                    else
                    {
                        chargeAmount += chargeSpeed * Time.deltaTime;
                        chargeMaterial.SetColor("_Color", Color.Lerp(Color.green, Color.red, chargeAmount / chargeSpeed));
                        if(chargeAmount >= 1)
                        {
                            charging = false;
                            chargeEffect.gameObject.SetActive(false);
                            FireLazer();
                        }
                    }
                }
                else if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.CHARGE_BOMB)
                {
                    chargeAmount += chargeSpeed * Time.deltaTime;
                    chargeMaterial.SetColor("_Color", Color.Lerp(Color.green, Color.red, chargeAmount / chargeSpeed));
                }
            }
        }
    }

    void OnDestroy()
    {
        InputManager.input.Player.Fire1.performed -= Fire1_performed;
        InputManager.input.Player.Fire1.canceled -= Fire1_canceled;
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
        if (!GameManager.Get().gamePaused && !GameManager.Get().gameOver)
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
        if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.NORMAL_BULLET)
        {
            shootTimer = 0;
        }
        else if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.CHARGE_BOMB)
        {
            chargeAmount = 0;
            charging = true;
            chargeEffect.gameObject.SetActive(true);
        }
        else if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.RAVER_LAZER)
        {
            chargeAmount = 0;
            charging = true;
            chargeEffect.gameObject.SetActive(true);
        }
    }
    
    private void Fire1_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.CHARGE_BOMB)
        {
            if (chargeAmount >= 1)
            {
                FirePowerBomb();
            }
            else
            {
                FireBullet();
            }
            charging = false;
            chargeEffect.gameObject.SetActive(false);
        }
        else if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.RAVER_LAZER)
        {
            if (lazer)
            {
                lazer.GetComponent<Lazer>().DeSpawn();
                lazer = null;
            }
        }
    }

    private void Fire2_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        int r = Random.Range(0, 4);
        if (r == 0)
        {
            orbiters.AddOrbiter(Orbiter.Type.TURRET);
        }
        else if (r == 1)
        {
            orbiters.AddOrbiter(Orbiter.Type.RAVER_LAZER);
        }
        else if (r == 2)
        {
            orbiters.AddOrbiter(Orbiter.Type.MISSILE);
        }
    }

    private void Fire2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        
    }

    private void Brake_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.Get().gamePaused && !GameManager.Get().gameOver)
        {
            if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ALL_RANGE)
            {
                if(strafeMode)
                {
                    targetSpeed = baseSpeed;
                    thrustColor = Color.cyan;
                    foreach (TrailRenderer trail in trails)
                    {
                        trail.emitting = false;
                    }

                    reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
                    reticle.rectTransform.position = reticlePosition;
                    strafeMode = false;
                }
                else
                {
                    targetSpeed = baseSpeed;
                    thrustColor = Color.cyan;
                    foreach (TrailRenderer trail in trails)
                    {
                        trail.emitting = true;
                    }
                    strafeMode = true;
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
        evadeTimer = 0;
        evading = true;
        if (evadeDirection == 1) evadeDirection = -1;
        else evadeDirection = 1;
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        camera.transform.position = position + (transform.up * 3) - (camera.transform.forward * camera.distance);
        controller.enabled = true;
    }

    void OnRailsMode()
    {
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        thruster.endColor = Color.Lerp(thruster.endColor, thrustColor, 5 * Time.deltaTime);


        OnRailsFollowTarget.transform.position += OnRailsFollowTarget.transform.forward * speed * Time.deltaTime;

        Vector2 steer = InputManager.input.Player.Steer.ReadValue<Vector2>();
        offset += new Vector3(steer.x, steer.y, 0) * Time.deltaTime;
        offset.x = Mathf.Clamp01(offset.x);
        offset.y = Mathf.Clamp01(offset.y);
        offset.z = speed / 4;
       

        Vector3 offsetWorld = Camera.main.ViewportToWorldPoint(offset);
        transform.position = offsetWorld;

        //Evading
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
            mesh.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(mesh.localEulerAngles.z, steer.x * -45, 10 * Time.deltaTime));
        }

        //Aiming
        if (aimingViaGamepad)
        {
            reticlePosition += InputManager.input.Player.Aim.ReadValue<Vector2>() * GameSettings.aimSensitivy * Time.deltaTime;
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

    void StrafeMode()
    {
        //Moving
        float moveX = InputManager.input.Player.Steer.ReadValue<Vector2>().x;
        float moveY = InputManager.input.Player.Steer.ReadValue<Vector2>().y;
        controller.Move(((transform.forward * moveY) + (transform.right * moveX)).normalized * baseSpeed * Time.deltaTime);

        //Aiming
        float lookX = InputManager.input.Player.Aim.ReadValue<Vector2>().x;
        float lookY = InputManager.input.Player.Aim.ReadValue<Vector2>().y;
        transform.rotation *= Quaternion.AngleAxis(lookX * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookY * turnSpeed * Time.deltaTime, Vector3.right);
    }

    void AllRangeMode()
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

        //Auto Level
        if (x == 0 && y == 0)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(transform.localEulerAngles.z, 0, 5 * Time.deltaTime));
        }

        //Evasion
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
            reticlePosition += InputManager.input.Player.Aim.ReadValue<Vector2>() * GameSettings.aimSensitivy * Time.deltaTime;
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
        GameObject obj = GameManager.Get().objectPool.SpawnFromObjectPool("bullet", bulletSpawn.position);
        if (obj != null)
        {
            Bullet b = obj.GetComponent<Bullet>();
            b.damage = firePower;
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
        GameObject obj = GameManager.Get().objectPool.SpawnFromObjectPool("missile", bulletSpawn.position);
        if (obj != null)
        {
            Missile m = obj.GetComponent<Missile>();
            m.damage = firePower * 3;
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
            lazer = GameManager.Get().objectPool.SpawnFromObjectPool("lazer", Vector3.zero).GetComponent<Lazer>();
            lazer.owner = mesh.gameObject;
            lazer.damage = firePower;

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
