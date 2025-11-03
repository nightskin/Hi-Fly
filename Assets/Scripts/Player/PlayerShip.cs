using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerShip : MonoBehaviour
{
    public enum RangedWeapon
    {
        CHARGE_MISSILE,
        MULTI_SHOT,
        RAVER_LAZER,
    }
    public RangedWeapon weapon = RangedWeapon.CHARGE_MISSILE;

    public enum MeleeWeapon
    {
        NONE,
        DRILL_DASHER,
    }
    [HideInInspector] public MeleeWeapon meleeWeapon = MeleeWeapon.NONE;

    //Necessary Components
    public HealthSystem health;
    public GameObject drillDasher;

    public PlayerCamera camera;
    [SerializeField] ParticleSystem chargeEffect;
    [SerializeField] TrailRenderer thruster;
    public Transform mesh;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] CharacterController controller;
    [SerializeField] Transform OnRailsFollowTarget;

    //Flight variables
    [HideInInspector] public float speed;
    float targetSpeed;
    Vector3 offset = Vector3.one *  0.5f;
    Color thrustColor = Color.cyan;
    [SerializeField][Min(1)] float turnSpeed = 5;
    [SerializeField] float baseSpeed = 50;
    [SerializeField] float boostSpeed = 200;
    [SerializeField] float acceleration = 10;
    //Barrel Rolls
    bool evading = false;
    int evadeDirection = 0;
    float evadeTimer;
    float evadeSpeed = 360 * 5;

    //Strafe Mode
    public bool strafeMode = false;
    [SerializeField] float baseStrafeSpeed = 25;
    [SerializeField] float maxStrafeSpeed = 100;
    float strafeSpeed;


    //For Shooting
    List<Transform> homingTargets = new List<Transform>();
    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;
    Vector2 reticlePosition;
    RaycastHit lockOn;


    [SerializeField] Material chargeMaterial;
    bool fireBtnHeldDown = false;
    float chargeAmount = 0;
    Lazer lazer = null;

    public float chargeSpeed = 1;
    public int firePower = 5;
    public int missilePower = 2;
    
    
    void Start()
    {
        Cursor.visible = false;
        mesh.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        mesh.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);

        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<PlayerCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;
        strafeSpeed = baseStrafeSpeed;
        InputManager.player.Shoot.performed += Shoot_performed;
        InputManager.player.Shoot.canceled += Shoot_canceled;
        InputManager.player.CenterCrosshair.performed += CenterCrosshair;
        InputManager.player.ToggleStrafeMode.performed += StrafeMode_performed;
        InputManager.player.Boost.performed += Boost_performed;
        InputManager.player.Boost.canceled += Boost_canceled;
        InputManager.player.Roll.performed += Roll_performed;
        InputManager.player.Dash.performed += Dash_performed;
        InputManager.player.Dash.canceled += Dash_canceled;

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
                if (strafeMode) StrafeControls();
                else AllRangeControls();
            }
            else if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ON_RAILS)
            {
                OnRailsControls();
            }
            
            //Shooting
            if (fireBtnHeldDown)
            {
                if (weapon == RangedWeapon.RAVER_LAZER)
                {
                    UpdateLazer();
                }
                else if (weapon == RangedWeapon.CHARGE_MISSILE)
                {
                    chargeAmount += chargeSpeed * Time.deltaTime;
                    chargeMaterial.SetColor("_Color", Color.Lerp(Color.green, Color.red, chargeAmount / chargeSpeed));
                }
                else if(weapon == RangedWeapon.MULTI_SHOT)
                {
                    
                }
            }
        }
    }
    void OnDestroy()
    {
        InputManager.player.Shoot.performed -= Shoot_performed;
        InputManager.player.Shoot.canceled -= Shoot_canceled;
        InputManager.player.CenterCrosshair.performed -= CenterCrosshair;
        InputManager.player.ToggleStrafeMode.performed -= StrafeMode_performed;
        InputManager.player.Boost.performed -= Boost_performed;
        InputManager.player.Boost.canceled -= Boost_canceled;
        InputManager.player.Roll.performed -= Roll_performed;
        InputManager.player.Dash.performed -= Dash_performed;
        InputManager.player.Dash.canceled -= Dash_canceled;
    }

    private void Boost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!GameManager.Get().gamePaused && !GameManager.Get().gameOver)
        {
            if(!strafeMode)
            {
                thruster.emitting = true;
                camera.boostEffect.Play();
                thrustColor = Color.red;
                targetSpeed = boostSpeed;
            }
        }
    }

    private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!GameManager.Get().gamePaused && !GameManager.Get().gameOver)
        {
            if(!strafeMode)
            {
                camera.boostEffect.Stop();
                thrustColor = Color.cyan;
                targetSpeed = baseSpeed;
            }
        }
    }
    
    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        fireBtnHeldDown = true;
        if (weapon == RangedWeapon.CHARGE_MISSILE)
        {
            chargeAmount = 0;
            chargeEffect.gameObject.SetActive(true);
            FireBullet();
        }
        else if(weapon == RangedWeapon.RAVER_LAZER)
        {
            FireLazer();
        }
        else if(weapon == RangedWeapon.MULTI_SHOT)
        {
            FireBullet();
        }
    }
    
    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        fireBtnHeldDown = false;
        if (weapon == RangedWeapon.CHARGE_MISSILE)
        {
            if (chargeAmount >= 1)
            {
                FireMissile();
            }
            chargeEffect.gameObject.SetActive(false);
        }
        else if (weapon == RangedWeapon.RAVER_LAZER)
        {
            if (lazer)
            {
                lazer.GetComponent<Lazer>().DeSpawn();
                lazer = null;
            }
        }
        else if(weapon == RangedWeapon.MULTI_SHOT)
        {
            if (homingTargets.Count > 0)
            {
                FireHomingBullets();
            }
            else
            {
                FireBullet();
            }

        }
    }
    
    private void StrafeMode_performed(UnityEngine.InputSystem.InputAction.CallbackContext context) 
    {
        if (!GameManager.Get().gamePaused && !GameManager.Get().gameOver)
        {
            if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ALL_RANGE)
            {
                if (strafeMode)
                {
                    SetStrafeMode(false);
                }
                else
                {
                    SetStrafeMode(true);
                }
            }
        }
    }
    
    private void CenterCrosshair(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
    }
    
    private void Roll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!strafeMode)
        {
            evadeTimer = 0;
            evading = true;
            if (evadeDirection == 1) evadeDirection = -1;
            else evadeDirection = 1;
        }
    }

    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(strafeMode)
        {
            strafeSpeed = maxStrafeSpeed;
        }
    }

    private void Dash_canceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (strafeMode)
        {
            strafeSpeed = baseStrafeSpeed;
        }
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        camera.transform.position = position + (transform.up * 3) - (camera.transform.forward * camera.distance);
        controller.enabled = true;
    }

    void SetStrafeMode(bool active)
    {
        strafeMode = active;
        if(strafeMode)
        {
            if (camera.boostEffect.isPlaying) camera.boostEffect.Stop();
            targetSpeed = baseSpeed;
            thrustColor = Color.cyan;
            reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
            reticle.rectTransform.position = reticlePosition;
            thruster.emitting = false;
            camera.followSpeed = camera.maxFollowSpeed;
        }
        else
        {
            targetSpeed = baseSpeed;
            thrustColor = Color.cyan;
            thruster.emitting = true;
            camera.followSpeed = camera.baseFollowSpeed;
        }
    }

    void OnRailsControls()
    {
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        thruster.endColor = Color.Lerp(thruster.endColor, thrustColor, 5 * Time.deltaTime);


        OnRailsFollowTarget.transform.position += OnRailsFollowTarget.transform.forward * speed * Time.deltaTime;

        Vector2 steer = InputManager.player.Steer.ReadValue<Vector2>();
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
        if(InputManager.controlScheme == InputManager.ControlScheme.GAMEPAD)
        {
            reticlePosition += InputManager.player.Aim.ReadValue<Vector2>() * GameSettings.aimSensitivy * Time.deltaTime;
            reticlePosition.x = Mathf.Clamp(reticlePosition.x, reticle.rectTransform.sizeDelta.x / 2, Screen.width - (reticle.rectTransform.sizeDelta.x / 2));
            reticlePosition.y = Mathf.Clamp(reticlePosition.y, reticle.rectTransform.sizeDelta.y / 2, Screen.height - (reticle.rectTransform.sizeDelta.y / 2));
            reticle.rectTransform.position = reticlePosition;
        }
        else if(InputManager.controlScheme == InputManager.ControlScheme.DESKTOP)
        {
            reticlePosition = Input.mousePosition;
            reticlePosition.x = Mathf.Clamp(reticlePosition.x, reticle.rectTransform.sizeDelta.x / 2, Screen.width - (reticle.rectTransform.sizeDelta.x / 2));
            reticlePosition.y = Mathf.Clamp(reticlePosition.y, reticle.rectTransform.sizeDelta.y / 2, Screen.height - (reticle.rectTransform.sizeDelta.y / 2));
            reticle.rectTransform.position = reticlePosition;
        }
        
    }

    void StrafeControls()
    {
        //Moving
        float x = InputManager.player.Steer.ReadValue<Vector2>().x;
        float z = InputManager.player.Steer.ReadValue<Vector2>().y;
        float y = InputManager.player.Ascend_Descend.ReadValue<float>();
        controller.Move(((transform.forward * z) + (transform.right * x) + (transform.up * y)).normalized * strafeSpeed * Time.deltaTime);

        mesh.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(mesh.localEulerAngles.z, x * -45, 10 * Time.deltaTime));


        //Aiming
        float lookX = InputManager.player.Aim.ReadValue<Vector2>().x;
        float lookY = InputManager.player.Aim.ReadValue<Vector2>().y;
        transform.rotation *= Quaternion.AngleAxis(lookX * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookY * turnSpeed * Time.deltaTime, Vector3.right);
    }

    void AllRangeControls()
    {
        //Forward Movement
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.deltaTime);
        controller.Move(transform.forward * speed * Time.deltaTime);
        thruster.endColor = Color.Lerp(thruster.endColor, thrustColor, 5 * Time.deltaTime);


        //steering
        float x = InputManager.player.Steer.ReadValue<Vector2>().x;
        float y = InputManager.player.Steer.ReadValue<Vector2>().y;


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
        if (InputManager.controlScheme == InputManager.ControlScheme.GAMEPAD)
        {
            reticlePosition += InputManager.player.Aim.ReadValue<Vector2>() * GameSettings.aimSensitivy * Time.deltaTime;
            reticlePosition.x = Mathf.Clamp(reticlePosition.x, reticle.rectTransform.sizeDelta.x / 2, Screen.width - (reticle.rectTransform.sizeDelta.x / 2));
            reticlePosition.y = Mathf.Clamp(reticlePosition.y, reticle.rectTransform.sizeDelta.y / 2, Screen.height - (reticle.rectTransform.sizeDelta.y / 2));
            reticle.rectTransform.position = reticlePosition;
        }
        else if(InputManager.controlScheme == InputManager.ControlScheme.DESKTOP)
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
        GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
        if (obj != null)
        {
            Bullet b = obj.GetComponent<Bullet>();
            //Set Needed Variables
            b.damage = firePower;
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

    void FireHomingBullets()
    {

    }

    void FireMissile()
    {
        //Initialize Bullet
        GameObject obj = GameManager.Get().objectPool.Spawn("missile", bulletSpawn.position);
        if (obj != null)
        {
            Missile m = obj.GetComponent<Missile>();
            m.damage = firePower * missilePower;
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
            lazer = GameManager.Get().objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
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

    void UpdateLazer()
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
            if (chargeAmount >= 1)
            {
                fireBtnHeldDown = false;
                chargeEffect.gameObject.SetActive(false);
                FireLazer();
            }
        }
    }
}
