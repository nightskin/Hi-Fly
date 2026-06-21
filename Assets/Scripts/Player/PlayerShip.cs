using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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

    [HideInInspector] public bool thrusting = false;
    float speed;
    float targetSpeed;
    Vector3 moveInput = Vector3.zero;
    Vector2 lookInput = Vector2.zero;
    float autoLevel = 0;

    [SerializeField][Min(1)] float turnSpeed = 100;
    [SerializeField] float baseSpeed = 50;
    [SerializeField] float boostSpeed = 200;
    [SerializeField] float acceleration = 10;

    //For Shooting
    public enum Weapon
    {
        BLASTER,
        LAZER,
    }
    Weapon equipedWeapon = Weapon.BLASTER;

    [SerializeField] Text weaponText;
    [SerializeField] Image reticle;
    Vector2 reticlePosition = new Vector2(0.5f,0.5f);
    [SerializeField] LayerMask lockOnLayer;
    RaycastHit lockOn;

    [HideInInspector] public List<HomingTarget> targets = new List<HomingTarget>();
    float chargeAmount = 0;
    Color chargeColor;
    Lazer lazer = null;

    [SerializeField] int baseFirePower = 3;
    [SerializeField] float blastRadius = 10;
    [SerializeField] int lazerPower = 1;
    [SerializeField] float lazerSpeed = 0.01f;
    [SerializeField] int maxTargets = 5;
    
    void Start()
    {
        for (int i = 0; i < maxTargets; i++)
        {
            var l = Instantiate(lockUI, hud);
            l.gameObject.SetActive(false);
            targets.Add(new HomingTarget(null, l));
        }


        Cursor.visible = false;
        mesh.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        mesh.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);

        targetSpeed = baseSpeed;
        weaponText.text = equipedWeapon.ToString();
        if(!camera) camera = Camera.main;
    }
    void FixedUpdate()
    {   
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

        if (InputManager.player.Shoot.IsPressed() && equipedWeapon == Weapon.BLASTER)
        {
            chargeAmount += Time.fixedDeltaTime;
            chargeColor = Color.Lerp(Color.green * 2, Color.orangeRed * 2, chargeAmount);
            chargeMaterial.SetColor("_Color", chargeColor);

            for(int i = 0; i < targets.Count; i++)
            {
                //add any new targets that have not already been added
                bool alreadyTargeted = false;
                for(int j = 0 ; j < targets.Count; j++)
                {
                    if (targets[j].followTarget == lockOn.transform)
                    {
                        alreadyTargeted = true;
                        break;
                    }
                }

                if (!targets[i].ui.activeSelf && !alreadyTargeted)
                {
                    targets[i].followTarget = lockOn.transform;
                    targets[i].ui.SetActive(true);
                }

                //update targets on screen
                if (targets[i].followTarget && targets[i].ui.activeSelf)
                {
                    Vector3 viewPortPos = Camera.main.WorldToViewportPoint(targets[i].followTarget.position);
                    if(viewPortPos.x > 0 &&  viewPortPos.x < 1 && viewPortPos.y > 0 && viewPortPos.y < 1 && viewPortPos.z > 0)
                    {
                        targets[i].ui.transform.position = Camera.main.WorldToScreenPoint(targets[i].followTarget.position);
                    }
                    else
                    {
                        targets[i].followTarget = null;
                        targets[i].ui.SetActive(false);
                    }
                }
                else
                {
                    targets[i].followTarget = null;
                    targets[i].ui.SetActive(false);
                }
            }

        }
    }
    void Update()
    {
        if (health.IsAlive() && !GameManager.Get().gamePaused)
        {
            if(thrusting) ThrustControls();
            else StrafeControls();

            //Auto Level
            if (InputManager.player.Steer.ReadValue<Vector2>().magnitude == 0  && InputManager.player.Aim.ReadValue<Vector2>().magnitude == 0 && transform.localEulerAngles.z != 0 && GameSettings.autoLevel)
            {
                autoLevel = 0;
            }
            if(autoLevel < 1)
            {
                autoLevel += 5 * Time.deltaTime;
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(transform.localEulerAngles.z, 0, autoLevel));
            }

            //Center CrossHair
            if(InputManager.player.CenterCrossHair.WasPressedThisFrame())
            {
                if(thrusting)
                {
                    reticle.rectTransform.anchoredPosition = Vector2.zero;
                    reticlePosition = new Vector2(0.5f,0.5f);
                } 
            }

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

            //Boosting
            if(InputManager.player.Boost.IsPressed())
            {
                targetSpeed = boostSpeed;
            }
            else
            {
                targetSpeed = baseSpeed;
            }

            //Shooting
            if(InputManager.player.Shoot.WasPressedThisFrame())
            {
                if (equipedWeapon == Weapon.BLASTER)
                {
                    chargeEffect.gameObject.SetActive(true);
                }
                else if (equipedWeapon == Weapon.LAZER)
                {
                    FireLazer();
                }
            }
            else if(InputManager.player.Shoot.WasReleasedThisFrame())
            {
                if (equipedWeapon == Weapon.LAZER)
                {
                    if (lazer)
                    {
                            lazer.GetComponent<Lazer>().endFire = true;
                            lazer = null;
                        }
                    }
                else if (equipedWeapon == Weapon.BLASTER)
                {
                    chargeEffect.gameObject.SetActive(false);
                    if (GetActiveTargets() > 0)
                    {
                        FireMultiBlaster();
                    }
                    else
                    {
                        FireBlaster();
                    }
                }
            }

            //Handles Lazer
            if(InputManager.player.Shoot.IsPressed() && equipedWeapon == Weapon.LAZER)
            {
                UpdateLazer();
            }

            //Swapping Weapons
            if(InputManager.player.ToggleWeapon.WasPressedThisFrame())
            {
                if(equipedWeapon == Weapon.BLASTER)
                {
                    if(chargeEffect.gameObject.activeSelf) chargeEffect.gameObject.SetActive(false);
                    equipedWeapon = Weapon.LAZER;
                }
                else if(equipedWeapon == Weapon.LAZER)
                {
                    if(lazer) lazer.endFire = true;
                    equipedWeapon = Weapon.BLASTER;
                }

                weaponText.text = equipedWeapon.ToString();               
            }
        }
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        controller.enabled = true;
    }

    int GetActiveTargets()
    {
        int i = 0;
        foreach(HomingTarget target in targets)
        {
            if(target.ui.activeSelf)
            {
                i++;
            }
        }
        return i;
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
        float turnX = InputManager.player.Steer.ReadValue<Vector2>().x * -45;
        mesh.localEulerAngles = new Vector3(0,0,turnX);

        //Aiming
        lookInput = InputManager.player.Aim.ReadValue<Vector2>();
        transform.rotation *= Quaternion.AngleAxis(lookInput.x, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookInput.y, Vector3.right);
    }

    void ThrustControls()
    {
        controller.Move(transform.forward * speed * Time.deltaTime);
        
        //veering left and right
        float turnX = InputManager.player.Steer.ReadValue<Vector2>().x * -45;
        mesh.localEulerAngles = new Vector3(0,0,turnX);

        //Steering
        lookInput = InputManager.player.Steer.ReadValue<Vector2>();
        transform.rotation *= Quaternion.AngleAxis(lookInput.x, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookInput.y, Vector3.right);

        //Aiming
        Vector2 aimInput = InputManager.player.Aim.ReadValue<Vector2>();
        reticlePosition += aimInput * Time.deltaTime;
        reticlePosition.x = Mathf.Clamp(reticlePosition.x,0,1);
        reticlePosition.y = Mathf.Clamp(reticlePosition.y,0,1);
        reticle.rectTransform.position = camera.ViewportToScreenPoint(reticlePosition);
    }

    void FireBlaster()
    {
        //Initialize Bullet
        GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
        if (obj)
        {
            Bullet b = obj.GetComponent<Bullet>();
            b.owner = mesh.gameObject;

            if(chargeAmount >= 1)
            {
                b.explosive = true;
                b.damage = baseFirePower * 5;
                b.blastRadius = blastRadius;
            }
            else
            {
                b.damage = baseFirePower;
                b.explosive = false;
                b.blastRadius = 0;
            }

            b.trail.material.SetColor("_Color", chargeColor);
        
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
    
    void FireMultiBlaster()
    {
        for(int i = 0; i < targets.Count; i++)
        {
            if(targets[i].followTarget && targets[i].ui.activeSelf)
            {
                GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                Bullet b = obj.GetComponent<Bullet>();
                //Set Needed Variables
                b.owner = mesh.gameObject;

                if(chargeAmount >= 1)
                {
                    b.explosive = true;
                    b.damage = baseFirePower * 5;
                    b.blastRadius = blastRadius;
                }
                else
                {
                    b.damage = baseFirePower;
                    b.explosive = false;
                    b.blastRadius = 0;
                }

                b.trail.material.SetColor("_Color", chargeColor);
                b.direction = Random.insideUnitSphere.normalized;
                b.homingTarget = targets[i].followTarget;
                targets[i].followTarget = null;
                targets[i].ui.SetActive(false);
            }
        }
        chargeAmount = 0;
    }

    void FireLazer()
    {
        if (!lazer)
        {
            lazer = GameManager.Get().objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
            lazer.owner = mesh.gameObject;
            lazer.damage = lazerPower;
            lazer.speed = lazerSpeed;

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
