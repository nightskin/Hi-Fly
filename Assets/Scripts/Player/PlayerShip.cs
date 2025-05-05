using UnityEngine;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviour
{
    public enum PowerUp
    {
        NONE,
        LAZER,
        MACHINE_GUN,
        CHARGE_BOMB,
    }
    public PowerUp powerUp;

    public HealthSystem health;
    [SerializeField] PlayerCamera camera;
    [SerializeField] TrailRenderer[] trails;
    Color thrustColor = Color.cyan;

    CharacterController controller;
    Transform bulletSpawn;
    
    [SerializeField] float turnSpeed = 5;
    [SerializeField] float baseSpeed = 10;
    [SerializeField] float boostSpeed = 50;
    [SerializeField] float acceleration = 10;

    float targetSpeed;
    public float speed;
    float zRot;

    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;
    bool aimingViaGamepad = false;
    Vector2 reticlePosition;
    RaycastHit lockOn;
    
    [SerializeField] float fireRate = 0.1f;
    ObjectPool bulletPool;
    ObjectPool lazerPool;

    GameObject lazer = null;
    float shootTimer = 0;
    
    public bool evading = false;
    int rollDirection = -1;
    float rollDuration = 1.5f;
    float rollSpeed = 2000;
    float rollTimer;

    //For On Rails Controls
    float distanceAlongPath = 0f;
    [SerializeField] Vector3 pathOffset = Vector3.zero;

    void Start()
    {
        Cursor.visible = false;
        GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor",GameManager.playerBodyColor);
        GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor",GameManager.playerStripeColor);
        bulletSpawn = transform.Find("BulletSpawn");


        health = GetComponent<HealthSystem>();
        bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
        lazerPool = GameObject.Find("LazerPool").GetComponent<ObjectPool>();
        if (trails.Length == 0) trails = GetComponentsInChildren<TrailRenderer>();
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<PlayerCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;


        InputManager.input.Player.PrimaryFire.performed += PrimaryFire_performed;
        InputManager.input.Player.BarrelRoll.performed += BarrelRoll_performed;
        InputManager.input.Player.Aim.performed += Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed += Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed += CenterCrosshair_performed;
        InputManager.input.Player.ToggleEngines.performed += ToggleEngines_performed;
        InputManager.input.Player.Boost.performed += Boost_performed;
        InputManager.input.Player.Boost.canceled += Boost_canceled;
        InputManager.input.Player.SecondaryFire.performed += SecondaryFire_performed;
        InputManager.input.Player.SecondaryFire.canceled += SecondaryFire_canceled;


    }
    
    void FixedUpdate()
    {
        Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(reticle.rectTransform.position);
        if (Physics.SphereCast(ray, 4, out lockOn, camera.GetComponent<Camera>().farClipPlane, lockOnLayer))
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
            if(GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE)
            {
                StandardMode();
            }
            else if(GameManager.playerMode == GameManager.PlayerMode.STRAFE_MODE)
            {
                StrafeMode();
            }
            else if(GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
            {
                OnRailsMode();
            }
        }
    }
    
    void OnDestroy()
    {
        InputManager.input.Player.PrimaryFire.performed -= PrimaryFire_performed;
        InputManager.input.Player.BarrelRoll.performed -= BarrelRoll_performed;
        InputManager.input.Player.Aim.performed -= Gamepad_Aim_performed;
        InputManager.input.Player.Mouse_Position.performed -= Mouse_Aim_performed;
        InputManager.input.Player.CenterCrosshair.performed -= CenterCrosshair_performed;
        InputManager.input.Player.ToggleEngines.performed -= ToggleEngines_performed;
        InputManager.input.Player.Boost.performed -= Boost_performed;
        InputManager.input.Player.Boost.canceled -= Boost_canceled;
        InputManager.input.Player.SecondaryFire.performed -= SecondaryFire_performed;
        InputManager.input.Player.SecondaryFire.canceled -= SecondaryFire_canceled;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 8)
        {
            GetComponent<HealthSystem>().TakeDamage(10);
        }
        if(other.tag == "Bounds")
        {
            Teleport(transform.position * -1);
        }
    }

    private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE || GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            camera.boostEffect.Stop();
            thrustColor = Color.cyan;
            targetSpeed = baseSpeed;
        }
    }

    private void Boost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.gamePaused && !GameManager.gameOver)
        {
            if(GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE || GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
            {
                camera.boostEffect.Play();
                thrustColor = Color.red;
                targetSpeed = boostSpeed;
            }
        }
    }

    private void PrimaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        FireBullet();
    }

    private void SecondaryFire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(powerUp == PowerUp.LAZER)
        {
            FireLazer();
        }
        else if(powerUp == PowerUp.MACHINE_GUN)
        {
            shootTimer = 0;
        }
    }

    private void SecondaryFire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(lazer)
        {
            lazer.GetComponent<Lazer>().DeSpawn();
        }
    }

    private void ToggleEngines_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.gamePaused && !GameManager.gameOver)
        {
            if (GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE)
            {
                //Set To Strafe Mode
                GameManager.playerMode = GameManager.PlayerMode.STRAFE_MODE;
                camera.boostEffect.Stop();
                thrustColor = Color.cyan;
                targetSpeed = 0;
                SetTrails(false);
                reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
                reticle.rectTransform.position = reticlePosition;
            }
            else if(GameManager.playerMode == GameManager.PlayerMode.STRAFE_MODE)
            {
                //Set To Standard Mode
                GameManager.playerMode = GameManager.PlayerMode.STANDARD_MODE;
                targetSpeed = baseSpeed;
                SetTrails(true);
            }
        }
    }
    
    private void BarrelRoll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE || GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
        {
            if (!evading)
            {
                rollDirection *= -1;
                rollTimer = rollDuration;
                evading = true;
            }
        }
    }

    private void CenterCrosshair_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(GameManager.playerMode == GameManager.PlayerMode.STANDARD_MODE || GameManager.playerMode == GameManager.PlayerMode.ON_RAILS_MODE)
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
        camera.transform.position = (position + camera.offset) - (camera.transform.forward * camera.distance);
        controller.enabled = true;
    }

    void StandardMode()
    {
        //Forward Movement
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        controller.Move(transform.forward * speed * Time.deltaTime);
        trails[0].endColor = Color.Lerp(trails[0].endColor, thrustColor, 5 * Time.deltaTime);

        //Steering
        if(evading)
        {
            zRot += rollDirection * rollSpeed * Time.deltaTime;
            Quaternion targetRot = Quaternion.Euler(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, zRot);
            transform.rotation = targetRot;
            if (rollTimer > 0)
            {
                rollTimer -= Time.deltaTime;
            }
            else
            {
                evading = false;
            }
        }
        else
        {
            zRot = InputManager.input.Player.Steer.ReadValue<Vector2>().x * -35;
            Quaternion targetRot = Quaternion.Euler(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, zRot);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
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

        //PowerUps
        if(InputManager.input.Player.SecondaryFire.IsPressed()) 
        {
            if(powerUp == PowerUp.LAZER)
            {
                MoveLazer();
            }
            else if(powerUp == PowerUp.MACHINE_GUN)
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
    
    void StrafeMode()
    {
        //Movement
        Vector2 moveInput = InputManager.input.Player.Steer.ReadValue<Vector2>();
        Vector3 moveDirection = (camera.transform.forward * moveInput.y + camera.transform.right * moveInput.x).normalized;

        controller.Move(moveDirection * baseSpeed * Time.deltaTime);

        //Steering
        zRot = InputManager.input.Player.Steer.ReadValue<Vector2>().x * -35;
        Quaternion targetRot = Quaternion.Euler(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, zRot);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        
        //Shooting lazer
        if (InputManager.input.Player.SecondaryFire.IsPressed())
        {
            if (powerUp == PowerUp.LAZER)
            {
                MoveLazer();
            }
            else if (powerUp == PowerUp.MACHINE_GUN)
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

    void OnRailsMode()
    {
        if (GameManager.onRailsPath)
        {
            transform.parent = camera.followTarget;

            // Rail Movement
            trails[0].endColor = Color.Lerp(trails[0].endColor, thrustColor, 5 * Time.deltaTime);
            speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);

            distanceAlongPath += speed * Time.deltaTime / GameManager.onRailsPathLength;
            camera.followTarget.position = GameManager.onRailsPath.EvaluatePosition(distanceAlongPath);

            if (distanceAlongPath > 1)
            {
                transform.parent = transform.root;
                GameManager.playerMode = GameManager.PlayerMode.STANDARD_MODE;
            }

            Vector3 nextPosition = GameManager.onRailsPath.EvaluatePosition(distanceAlongPath + 0.05f);
            Vector3 direction = nextPosition - camera.followTarget.position;
            camera.followTarget.rotation = Quaternion.LookRotation(direction);

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
            if (evading)
            {
                zRot += rollDirection * rollSpeed * Time.deltaTime;
                Quaternion targetRot = Quaternion.Euler(camera.followTarget.localEulerAngles.x, camera.followTarget.localEulerAngles.y, zRot);
                transform.rotation = targetRot;
                if (rollTimer > 0)
                {
                    rollTimer -= Time.deltaTime;
                }
                else
                {
                    evading = false;
                }
            }
            else
            {
                zRot = InputManager.input.Player.Steer.ReadValue<Vector2>().x * -35;
                Quaternion targetRot = Quaternion.Euler(camera.followTarget.localEulerAngles.x, camera.followTarget.localEulerAngles.y, zRot);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
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

            //PowerUps
            if (InputManager.input.Player.SecondaryFire.IsPressed())
            {
                if (powerUp == PowerUp.LAZER)
                {
                    MoveLazer();
                }
                else if (powerUp == PowerUp.MACHINE_GUN)
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
        else
        {
            GameManager.playerMode = GameManager.PlayerMode.STANDARD_MODE;
            transform.parent = transform.root;
        }
    }

    void FireBullet()
    {
        if(!GameManager.gameOver && !GameManager.gamePaused)
        {
            //Initialize Bullet
            GameObject obj = bulletPool.Spawn(bulletSpawn.position);
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
                            if(hit.transform.tag == "Bounds")
                            {
                                b.direction = ray.direction;
                            }
                            else
                            {
                                b.direction = (hit.point - bulletSpawn.position).normalized;
                            }
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
            lazer = lazerPool.Spawn(Vector3.zero);
            if (lazer != null)
            {
                Lazer l = lazer.GetComponent<Lazer>();
                l.owner = gameObject;
                Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(reticle.rectTransform.position);
                if (Physics.Raycast(ray, out RaycastHit hit, camera.GetComponent<Camera>().farClipPlane, lockOnLayer))
                {
                    l.direction = (hit.point - (transform.position + transform.forward)).normalized;
                }
                else
                {
                    l.direction = ray.direction;
                }
            }
        }

    }

    void MoveLazer()
    {
        if (lazer != null && !GameManager.gamePaused && !GameManager.gameOver)
        {
            Lazer l = lazer.GetComponent<Lazer>();
            Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(reticle.rectTransform.position);
            if (Physics.Raycast(ray, out RaycastHit hit, camera.GetComponent<Camera>().farClipPlane, lockOnLayer))
            {
                l.direction = (hit.point - (transform.position + transform.forward)).normalized;
            }
            else
            {
                l.direction = ray.direction;
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
