using UnityEngine;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviour
{
    GameManager gameManager;
    public HealthSystem health;
    [SerializeField] PlayerCamera camera;
    [SerializeField] TrailRenderer[] trails;
    Color thrustColor = Color.cyan;

    CharacterController controller;
    Transform bulletSpawn;
    
    [SerializeField][Min(1)] float turnSpeed = 5;
    [SerializeField] float baseSpeed = 10;
    [SerializeField] float boostSpeed = 50;
    [SerializeField] float acceleration = 10;

    float targetSpeed;
    public float speed;

    //For Shooting
    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;
    bool aimingViaGamepad = false;
    Vector2 reticlePosition;
    RaycastHit lockOn;
    
    [SerializeField] float fireRate = 0.1f;
    ObjectPool objectPool;

    Lazer lazer = null;
    float shootTimer = 0;
    

    //For On Rails Controls
    Vector3 pathOffset = Vector3.zero;
    [SerializeField] float distanceAlongPath = 0;

    void Start()
    {
        gameManager = transform.root.GetComponent<GameManager>();


        Cursor.visible = false;
        GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor",GameManager.playerBodyColor);
        GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor",GameManager.playerStripeColor);
        bulletSpawn = transform.Find("BulletSpawn");


        health = GetComponent<HealthSystem>();
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
        if (trails.Length == 0) trails = GetComponentsInChildren<TrailRenderer>();
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<PlayerCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;


        InputManager.input.Player.Fire.performed += PrimaryFire_performed;
        InputManager.input.Player.Fire.canceled += Fire_canceled;
        InputManager.input.Player.Aim.performed += Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed += Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed += CenterCrosshair_performed;
        InputManager.input.Player.Brake.performed += Brake_performed;
        InputManager.input.Player.Boost.performed += Boost_performed;
        InputManager.input.Player.Boost.canceled += Boost_canceled;
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
        if(health.IsAlive() && !GameManager.gamePaused)
        {
            if(GameManager.playerMode == GameManager.PlayerMode.All_RANGE_MODE)
            {
                AllRangeControls();
            }
            else if(GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
            {
                OnRailsControls();
            }
            UpdatePowerUps();
        }
    }
    
    void OnDestroy()
    {
        InputManager.input.Player.Fire.performed -= PrimaryFire_performed;
        InputManager.input.Player.Aim.performed -= Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed -= Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed -= CenterCrosshair_performed;
        InputManager.input.Player.Brake.performed -= Brake_performed;
        InputManager.input.Player.Boost.performed -= Boost_performed;
        InputManager.input.Player.Boost.canceled -= Boost_canceled;
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
            SetTrails(true);
        }
    }

    private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        camera.boostEffect.Stop();
        thrustColor = Color.cyan;
        targetSpeed = baseSpeed;
    }
    
    private void PrimaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(GameManager.currentPowerUp == GameManager.PowerUps.NONE)
        {
            FireBullet();
        }
        else if(GameManager.currentPowerUp == GameManager.PowerUps.MISSILE)
        {
            FireMissile();
        }
        else if(GameManager.currentPowerUp == GameManager.PowerUps.RAPID_FIRE)
        {
            shootTimer = 0;
        }
        else if(GameManager.currentPowerUp == GameManager.PowerUps.LAZER)
        {
            FireLazer();
        }
    }

    private void Fire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
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
                SetTrails(false);
            }
            else
            {
                camera.boostEffect.Stop();
                targetSpeed = baseSpeed;
                SetTrails(true);
            }
        }
    }
    
    private void CenterCrosshair_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(GameManager.playerMode == GameManager.PlayerMode.All_RANGE_MODE || GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
            reticle.rectTransform.position = reticlePosition;
        }
    }

    private void Gamepad_Aim_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        aimingViaGamepad = true;
    }

    private void Mouse_Aim_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        aimingViaGamepad = false;
    }
  
    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        camera.transform.position = position + (transform.up * 3) - (camera.transform.forward * camera.distance);
        controller.enabled = true;
    }

    void AllRangeControls()
    {
        //Forward Movement
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        controller.Move(transform.forward * speed * Time.deltaTime);
        trails[0].endColor = Color.Lerp(trails[0].endColor, thrustColor, 5 * Time.deltaTime);

        float zRot = InputManager.input.Player.Steer.ReadValue<Vector2>().x * -35;
        Quaternion targetRot = Quaternion.Euler(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, camera.transform.localEulerAngles.z + zRot);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);


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
    
    void OnRailsControls()
    {
        if (distanceAlongPath < 1 && GameManager.splinePathLength > 0)
        {
            //Handles Boosting
            trails[0].endColor = Color.Lerp(trails[0].endColor, thrustColor, 5 * Time.deltaTime);
            speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
            
            // Rail Movement
            transform.parent = camera.followTarget;
            Vector3 followTargetPosition = GameManager.splinePath.EvaluatePosition(distanceAlongPath);
            distanceAlongPath += (speed / GameManager.splinePathLength) * Time.deltaTime;
            camera.followTarget.position = Vector3.MoveTowards(camera.followTarget.position, followTargetPosition, speed * Time.deltaTime);
            Quaternion followTargetRotation = Quaternion.LookRotation(followTargetPosition - camera.followTarget.position);
            camera.followTarget.rotation = Quaternion.Lerp(camera.followTarget.rotation, followTargetRotation, 10 * Time.deltaTime);

            // Rail Offset
            Vector2 moveInput = InputManager.input.Player.Steer.ReadValue<Vector2>();
            Vector3 moveDirection = new Vector3(moveInput.x, moveInput.y, 0);
            pathOffset += moveDirection * baseSpeed * Time.deltaTime;
            

            if(targetSpeed == baseSpeed)
            {
                pathOffset.x = Mathf.Clamp(pathOffset.x, -10, 10);
                pathOffset.y = Mathf.Clamp(pathOffset.y, -3, 10);
            }
            else if(targetSpeed == boostSpeed)
            {
                pathOffset.x = Mathf.Clamp(pathOffset.x, -20, 20);
                pathOffset.y = Mathf.Clamp(pathOffset.y, -5, 15);
            }

            transform.localPosition = pathOffset;


            //Steering
            float zRot = InputManager.input.Player.Steer.ReadValue<Vector2>().x * -35;
            Quaternion targetRot = Quaternion.Euler(camera.followTarget.localEulerAngles.x, camera.followTarget.localEulerAngles.y, camera.followTarget.localEulerAngles.z + zRot);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);


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
        else
        {
            GameManager.playerMode = GameManager.PlayerMode.All_RANGE_MODE;
            transform.parent = transform.root;
            distanceAlongPath = 0;
            pathOffset = Vector3.zero;
        }
    }

    void FireBullet()
    {
        if(!GameManager.gameOver && !GameManager.gamePaused)
        {
            //Initialize Bullet
            GameObject obj = objectPool.Spawn("bullet", bulletSpawn.position);
            if (obj != null)
            {
                Bullet b = obj.GetComponent<Bullet>();
                //Set Needed Variables
                b.owner = gameObject;
            
                if (reticle.color == Color.red)
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
    }

    void FireMissile()
    {
        if (!GameManager.gameOver && !GameManager.gamePaused)
        {
            //Initialize Missile
            GameObject obj = objectPool.Spawn("missile", bulletSpawn.position);
            if (obj != null)
            {
                Missile b = obj.GetComponent<Missile>();
                //Set Needed Variables
                b.owner = gameObject;
            
                if (reticle.color == Color.red)
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
    }

    void FireLazer()
    {
        if (!GameManager.gameOver && !GameManager.gamePaused)
        {
            lazer = objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
            if (lazer != null)
            {
                lazer.owner = gameObject;
            
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

    void UpdateLazer()
    {
        if (!GameManager.gamePaused && !GameManager.gameOver)
        {
            if (reticle.color == Color.red)
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

    void UpdatePowerUps()
    {
        //PowerUps
        if (GameManager.currentPowerUp == GameManager.PowerUps.LAZER)
        {
            if(lazer != null)
            {
                UpdateLazer();
            }
        }
        else if (GameManager.currentPowerUp == GameManager.PowerUps.RAPID_FIRE)
        {
            if(InputManager.input.Player.Fire.IsPressed())
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

        }
    }
    
    void SetTrails(bool active)
    {
        foreach (TrailRenderer trail in trails)
        {
            trail.emitting = active;
        }
    }
}
