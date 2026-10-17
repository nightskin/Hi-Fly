using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HomingTarget
{
    public Transform followTarget;
    public GameObject ui;

    public HomingTarget(Transform transform, GameObject lockUI)
    {
        followTarget = transform;
        ui = lockUI;
    }

}

public class PlayerShip : MonoBehaviour
{
    //Necessary Components
    public HealthSystem health;
    public Camera camera;
    [SerializeField] GameObject boostEffect;
    [SerializeField] GameObject lockUI;
    [SerializeField] Transform hud;
    [SerializeField] Material chargeMaterial;
    [SerializeField] ParticleSystem chargeEffect;
    [SerializeField] TrailRenderer thruster;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] Transform mesh;
    [SerializeField] CharacterController controller;

    bool usingMouse = false;
    bool thrusting = true;
    float speed;
    float targetSpeed;
    Vector3 moveInput = Vector3.zero;

    [SerializeField][Min(1)] float turnSpeed = 100;
    [SerializeField] float baseSpeed = 50;
    [SerializeField] float boostSpeed = 200;
    [SerializeField] float acceleration = 10;

    //For Shooting
    public Weapon equiped = Weapon.BLASTER;
    public enum Weapon
    {
        LAZER,
        BLASTER,
    }

    [SerializeField] Text weaponText;
    [SerializeField] Image reticle;
    Vector2 reticlePosition = new Vector2(0.5f,0.5f);
    [SerializeField] LayerMask lockOnLayer;
    RaycastHit lockOn;

    [HideInInspector] public List<HomingTarget> targets = new List<HomingTarget>();
    float chargeAmount = 0;
    Lazer lazer = null;

    bool justDied;

    
    void Start()
    {
        justDied = false;
        Cursor.visible = false;
        mesh.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        mesh.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);

        targetSpeed = baseSpeed;
        weaponText.text = equiped.ToString();
        if(!camera) camera = Camera.main;
    }
    void FixedUpdate()
    {
        //Boosting
        if(InputManager.player.Boost.IsPressed() && thrusting)
        {
            targetSpeed = boostSpeed;
        }
        else
        {
            targetSpeed = baseSpeed;
        }
        
        //Handles acceleration
        speed = Mathf.Lerp(speed, targetSpeed, acceleration * Time.fixedDeltaTime);

        //Handles targeting
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
            if(thrusting) ThrustControls();
            else StrafeControls();


            //TogglesThrustMode
            if(InputManager.player.ToggleThrustMode.WasPressedThisFrame())
            {
                if(thrusting)
                {
                    thrusting = false;
                    thruster.emitting = false;
                    camera.transform.parent = transform;
                    reticle.rectTransform.anchoredPosition = Vector2.zero;
                    reticlePosition = new Vector2(0.5f,0.5f);
                }
                else
                {
                    thrusting = true;
                    thruster.emitting = true;
                    camera.transform.parent = transform.parent;
                }
            }

            //Shooting
            if(InputManager.player.Shoot.WasPressedThisFrame())
            {
                if (equiped == Weapon.BLASTER)
                {
                    chargeEffect.gameObject.SetActive(true);
                }
                else if (equiped == Weapon.LAZER)
                {
                    FireLazer();
                }
            }
            else if(InputManager.player.Shoot.WasReleasedThisFrame())
            {
                if (equiped == Weapon.LAZER)
                {
                    if (lazer)
                    {
                        lazer.GetComponent<Lazer>().endFire = true;
                        lazer = null;
                    }
                }
                else if (equiped == Weapon.BLASTER)
                {
                    chargeEffect.gameObject.SetActive(false);
                    FireBlaster();
                }
            }

            //Handles Lazer
            if(InputManager.player.Shoot.IsPressed() && equiped == Weapon.LAZER)
            {
                UpdateLazer();
            }
            //Handles Blaster
            else if(InputManager.player.Shoot.IsPressed() && equiped == Weapon.BLASTER)
            {
                chargeAmount += Time.deltaTime;
            }

            //Swapping Weapons
            if(InputManager.player.ToggleWeapon.WasPressedThisFrame())
            {
                if(equiped == Weapon.BLASTER)
                {
                    if(chargeEffect.gameObject.activeSelf) chargeEffect.gameObject.SetActive(false);
                    equiped = Weapon.LAZER;
                }
                else if(equiped == Weapon.LAZER)
                {
                    if(lazer) lazer.endFire = true;
                    equiped = Weapon.BLASTER;
                }

                weaponText.text = equiped.ToString();               
            }
        }
        else
        {
            if(!justDied)
            {
                GameManager.Get().objectPool.Spawn("explosion", transform.position);
                mesh.gameObject.SetActive(false);
                GameManager.Get().gameOver = true;
                justDied = true;
            }

        }
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    void StrafeControls()
    {
        //Moving
        moveInput.x = InputManager.player.Steer.ReadValue<Vector2>().x;
        moveInput.y = InputManager.player.Steer.ReadValue<Vector2>().y;
        moveInput.z = InputManager.player.Ascend_Descend.ReadValue<float>();

        if(moveInput.y > 0)
        {
            thruster.emitting = true;
        }
        else
        {
            thruster.emitting = false;
        }

        controller.Move(((transform.forward * moveInput.y) + (transform.right * moveInput.x) + (transform.up * moveInput.z)).normalized * speed * Time.deltaTime);

        //veering left and right
        float turnX = moveInput.x * -45;
        mesh.localEulerAngles = new Vector3(0,0, Mathf.LerpAngle(mesh.localEulerAngles.z,turnX, 10 * Time.deltaTime));

        //Aiming
        Vector2 lookXY = InputManager.player.Aim.ReadValue<Vector2>();
        float lookZ = InputManager.player.Rotate.ReadValue<float>();
        transform.rotation *= Quaternion.AngleAxis(lookXY.x * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookXY.y * turnSpeed * Time.deltaTime, Vector3.right);
        transform.rotation *= Quaternion.AngleAxis(lookZ * turnSpeed * Time.deltaTime, Vector3.forward);
    }

    void ThrustControls()
    {
        controller.Move(transform.forward * speed * Time.deltaTime);
    
        //veering left and right
        float turnX = InputManager.player.Steer.ReadValue<Vector2>().x * -45;
        mesh.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(mesh.localEulerAngles.z,turnX, 10 * Time.deltaTime));

        //Steering
        Vector2 lookXY = InputManager.player.Steer.ReadValue<Vector2>();
        float lookZ = InputManager.player.Rotate.ReadValue<float>();
        transform.rotation *= Quaternion.AngleAxis(lookXY.x * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(lookXY.y * turnSpeed * Time.deltaTime, Vector3.right);
        transform.rotation *= Quaternion.AngleAxis(lookZ * turnSpeed * Time.deltaTime, Vector3.forward);


        if(Gamepad.current.rightStick.ReadValue().magnitude > 0)
        {
            usingMouse = false;
        }
        else if(Mouse.current.delta.ReadValue().magnitude > 0)
        {
            usingMouse = true;
        }


        //Aiming
        if(usingMouse)
        {
            reticlePosition = Mouse.current.position.ReadValue();
            reticle.rectTransform.position = reticlePosition;
        }
        else
        {
            Vector2 aimInput = InputManager.player.Aim.ReadValue<Vector2>();
            reticlePosition += aimInput * Time.deltaTime;
            reticlePosition.x = Mathf.Clamp(reticlePosition.x,0,1);
            reticlePosition.y = Mathf.Clamp(reticlePosition.y,0,1);
            reticle.rectTransform.position = camera.ViewportToScreenPoint(reticlePosition);
        }


        //Center CrossHair
        if(InputManager.player.CenterCrossHair.WasPressedThisFrame())
        {
            reticle.rectTransform.anchoredPosition = Vector2.zero;
            reticlePosition = new Vector2(0.5f,0.5f);
        }
    }

    void FireBlaster()
    {
        //Initialize Bullet
        GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
        if (obj)
        {
            targets[0].ui.SetActive(false);
            targets[0].followTarget = null;
            Bullet b = obj.GetComponent<Bullet>();
            b.owner = mesh.gameObject;

            if(chargeAmount >= 1)
            {
                b.isPowerBomb = true;
            }
            else
            {
                b.isPowerBomb = false;
            }

            if (lockOn.collider)
            {
                b.homingTarget = lockOn.collider.transform;
            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(reticle.rectTransform.position);
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
            chargeAmount = 0;
        }
    }
    
    void FireLazer()
    {
        if (!lazer)
        {
            lazer = GameManager.Get().objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
            lazer.owner = mesh.gameObject;

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
    }
}
