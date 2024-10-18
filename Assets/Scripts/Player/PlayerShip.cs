using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShip : MonoBehaviour
{
    public enum Weapon
    {
        BULLET,
        LAZER
    }
    public Weapon equipped;

    public HealthSystem health;

    [SerializeField] ThirdPersonCamera camera;
    [SerializeField] TrailRenderer[] trails;
    Color thrustColor = Color.cyan;
    CharacterController controller;


    [SerializeField] float turnSpeed = 5;
    [SerializeField] float baseSpeed = 10;
    [SerializeField] float boostSpeed = 50;
    [SerializeField] float acceleration = 10;

    float targetSpeed;
    float speed;
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

    void Start()
    {
        GetComponent<MeshRenderer>().materials[0].color = Settings.playerBodyColor;
        GetComponent<MeshRenderer>().materials[1].color = Settings.playerStripeColor;


        health = GetComponent<HealthSystem>();
        bulletPool = GameObject.Find("BulletPool").GetComponent<ObjectPool>();
        lazerPool = GameObject.Find("LazerPool").GetComponent<ObjectPool>();
        if (trails.Length == 0) trails = GetComponentsInChildren<TrailRenderer>();
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<ThirdPersonCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;


        InputManager.shipInput.actions.Shoot.performed += Shoot_performed;
        InputManager.shipInput.actions.Shoot.canceled += Shoot_canceled;
        InputManager.shipInput.actions.BarrelRoll.performed += BarrelRoll_performed;
        InputManager.shipInput.actions.Gamepad_Aim.performed += Gamepad_Aim_performed;
        InputManager.shipInput.actions.Mouse_Aim.performed += Mouse_Aim_performed;
        InputManager.shipInput.actions.CenterCrosshair.performed += CenterCrosshair_performed;
        InputManager.shipInput.actions.ToggleEngines.performed += ToggleEngines_performed;
        InputManager.shipInput.actions.Boost.performed += Boost_performed;
        InputManager.shipInput.actions.Boost.canceled += Boost_canceled;
        InputManager.shipInput.actions.ToggleWeapons.performed += ToggleWeapons_performed;
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
        if(health.IsAlive())
        {
            Controls();
        }
    }
    
    void OnDisable()
    {
        InputManager.shipInput.actions.BarrelRoll.performed -= BarrelRoll_performed;
        InputManager.shipInput.actions.CenterCrosshair.performed -= CenterCrosshair_performed;
        InputManager.shipInput.actions.ToggleEngines.performed -= ToggleEngines_performed;
        InputManager.shipInput.actions.Boost.performed -= Boost_performed;
        InputManager.shipInput.actions.Boost.canceled -= Boost_canceled;
    }
    
    private void Boost_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(targetSpeed > 0)
        {
            camera.boostEffect.Stop();
            thrustColor = Color.cyan;
            targetSpeed = baseSpeed;
        }
    }

    private void Boost_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(targetSpeed > 0)
        {
            camera.boostEffect.Play();
            thrustColor = Color.red;
            targetSpeed = boostSpeed;
        }
    }

    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(equipped == Weapon.BULLET)
        {
            shootTimer = 0;
        }
        else if(equipped == Weapon.LAZER)
        {
            FireLazer();
        }
    }

    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(lazer)
        {
            lazer.GetComponent<Lazer>().DeSpawn();
        }
    }

    private void ToggleEngines_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!InputManager.shipInput.actions.Boost.IsPressed())
        {
            if (targetSpeed > 0)
            {
                targetSpeed = 0;
                SetTrails(false);
            }
            else
            {
                targetSpeed = baseSpeed;
                SetTrails(true);
            }
        }

    }

    private void ToggleWeapons_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (equipped == Weapon.BULLET) 
        { 
            equipped = Weapon.LAZER; 
        }
        else if (equipped == Weapon.LAZER)
        {
            if(lazer)lazer.GetComponent<Lazer>().DeSpawn();
            equipped = Weapon.BULLET;
        }
    }

    private void BarrelRoll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!evading)
        {
            rollDirection *= -1;
            rollTimer = rollDuration;
            evading = true;
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
  
    void Controls()
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
            zRot = InputManager.shipInput.actions.Move.ReadValue<Vector2>().x * -35;
            Quaternion targetRot = Quaternion.Euler(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, zRot);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }


        //Aiming
        if(aimingViaGamepad) 
        {
            reticlePosition += InputManager.shipInput.actions.Gamepad_Aim.ReadValue<Vector2>() * Settings.aimSense * Time.deltaTime;
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

        //Shooting
        if (InputManager.shipInput.actions.Shoot.IsPressed())
        {
            if(equipped == Weapon.BULLET)
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
            else if(equipped == Weapon.LAZER)
            {
                MoveLazer();
            }
        }
    }
    
    void FireBullet()
    {
        //Initialize Bullet
        Vector3 bulletSpawn = transform.position + transform.forward;
        GameObject obj = bulletPool.Spawn(bulletSpawn);
        if(obj != null) 
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
                    if(hit.transform.gameObject == b.owner)
                    {
                        b.direction = ray.direction;
                    }
                    else
                    {
                        b.direction = (hit.point - bulletSpawn).normalized;
                    }
                }
                else
                {
                    b.direction = ray.direction;
                }
            }
        }
    }

    void FireLazer()
    {
        lazer = lazerPool.Spawn(Vector3.zero);
        if(lazer != null)
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

    void MoveLazer()
    {
        if (lazer != null)
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
