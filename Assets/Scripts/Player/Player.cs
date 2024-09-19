using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public HealthSystem health;

    [SerializeField] PlayerCamera camera;
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
        if (trails.Length == 0) trails = GetComponentsInChildren<TrailRenderer>();
        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        if (!camera) camera = Camera.main.GetComponent<PlayerCamera>();
        controller = GetComponent<CharacterController>();
        targetSpeed = baseSpeed;


        InputManager.input.actions.Shoot.performed += Shoot_performed;
        InputManager.input.actions.BarrelRoll.performed += BarrelRoll_performed;
        InputManager.input.actions.Gamepad_Aim.performed += Gamepad_Aim_performed;
        InputManager.input.actions.Mouse_Aim.performed += Mouse_Aim_performed;
        InputManager.input.actions.CenterCrosshair.performed += CenterCrosshair_performed;
        InputManager.input.actions.ToggleEngines.performed += ToggleEngines_performed;
        InputManager.input.actions.Boost.performed += Boost_performed;
        InputManager.input.actions.Boost.canceled += Boost_canceled;

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
        InputManager.input.actions.BarrelRoll.performed -= BarrelRoll_performed;
        InputManager.input.actions.CenterCrosshair.performed -= CenterCrosshair_performed;
        InputManager.input.actions.ToggleEngines.performed -= ToggleEngines_performed;
        InputManager.input.actions.Boost.performed -= Boost_performed;
        InputManager.input.actions.Boost.canceled -= Boost_canceled;
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
        shootTimer = 0;
    }

    private void ToggleEngines_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!InputManager.input.actions.Boost.IsPressed())
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
            zRot = InputManager.input.actions.Move.ReadValue<Vector2>().x * -35;
            Quaternion targetRot = Quaternion.Euler(camera.transform.localEulerAngles.x, camera.transform.localEulerAngles.y, zRot);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }


        //Aiming
        if(aimingViaGamepad) 
        {
            reticlePosition += InputManager.input.actions.Gamepad_Aim.ReadValue<Vector2>() * Settings.aimSense * Time.deltaTime;
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
        if (InputManager.input.actions.Shoot.IsPressed())
        {
            if(shootTimer > 0)
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
                    if(hit.transform.gameObject == gameObject)
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

    public void SetTrails(bool active)
    {
        foreach (TrailRenderer trail in trails)
        {
            trail.emitting = active;
        }
    }
}
