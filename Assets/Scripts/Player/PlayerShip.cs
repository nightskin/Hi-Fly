using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public GameObject drillDasher;
    public PlayerCamera camera;
    public Transform mesh;
    [SerializeField] GameObject lockUI;
    [SerializeField] Transform hud;
    [SerializeField] Material chargeMaterial;
    [SerializeField] ParticleSystem chargeEffect;
    [SerializeField] Transform thruster;
    [SerializeField] Material thrusterMaterial;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] CharacterController controller;
    [SerializeField] Transform OnRailsFollowTarget;

    //Flight variables
    float thrustScale = 0.25f;
    float normalThrusterScale = 0.25f;
    float boostThrusterScale = 2;
    [HideInInspector] public float speed;
    float thrustSpeed;
    Vector3 steer;
    
    [HideInInspector] public bool boosting = false;
    [SerializeField][Min(1)] float turnSpeed = 100;
    [SerializeField] float baseSpeed = 50;
    [SerializeField] float boostSpeed = 200;
    [SerializeField] float acceleration = 10;
    Vector3 offset = Vector3.one *  0.5f;

    //Barrel Rolls
    [HideInInspector] public bool evading = false;
    int evadeDirection = 0;
    float evadeTimer;
    float evadeSpeed = 360 * 5;

    //Strafe Mode
    [HideInInspector] public bool strafeMode = false;

    //For Shooting
    public enum Weapon
    {
        CHARGE_BOMB,
        MULTI_SHOT,
        RAVER_LAZER,
    }
    public Weapon rangedWeapon = Weapon.CHARGE_BOMB;
    int rangedWeaponIndex;
    
    [SerializeField] TextMeshProUGUI weaponText;
    [SerializeField] Image reticle;
    [SerializeField] LayerMask lockOnLayer;
    Vector2 reticlePosition;
    RaycastHit lockOn;

    [HideInInspector] public List<HomingTarget> targets = new List<HomingTarget>();
    float chargeAmount = 0;
    Lazer lazer = null;
    
    public float chargeSpeed = 0.5f;
    public int baseFirePower = 3;

    public int missileMult = 2;
    public float blastRadius = 10;

    public int lazerPower = 1;
    public float lazerSpeed = 0.01f;
    
    public int maxTargets = 5;
    [HideInInspector] public bool explodingBullets = false;
    
    void Start()
    {
        for (int i = 0; i < maxTargets; i++)
        {
            var l = Instantiate(lockUI, hud);
            l.gameObject.SetActive(false);
            targets.Add(new HomingTarget(null, l));
        }

        rangedWeaponIndex = (int)rangedWeapon;
        weaponText.text = " Weapon: " + rangedWeapon.ToString();

        Cursor.visible = false;
        mesh.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        mesh.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);

        reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
        reticle.rectTransform.position = reticlePosition;
        thrustSpeed = baseSpeed;

        InputManager.player.Shoot.performed += Shoot_performed;
        InputManager.player.Shoot.canceled += Shoot_canceled;
        InputManager.player.CenterCrosshair.performed += CenterCrosshair;
        InputManager.player.ToggleStrafeMode.performed += StrafeMode_performed;
        InputManager.player.Roll.performed += Roll_performed;
        InputManager.player.ToggleWeapon.performed += ToggleWeapon_performed;

        if (GameManager.Get().playerMovement == GameManager.PlayerMovement.ON_RAILS)
        {
            transform.parent = OnRailsFollowTarget;
        }
    }
    void FixedUpdate()
    {
        speed = Mathf.Lerp(speed, thrustSpeed, acceleration * Time.fixedDeltaTime);
        thruster.localScale = Vector3.Lerp(thruster.localScale, new Vector3(thruster.localScale.x, thruster.localScale.y, thrustScale), 5 * Time.fixedDeltaTime);


        Ray ray = Camera.main.ScreenPointToRay(reticle.rectTransform.position);
        if (Physics.SphereCast(ray, 4, out lockOn, Camera.main.farClipPlane, lockOnLayer))
        {
            reticle.color = Color.red;

            if (rangedWeapon == Weapon.MULTI_SHOT && InputManager.player.Shoot.IsPressed())
            {
                for (int i = 0; i < targets.Count; i++)
                {
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
                }
            }
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

            steer.x = InputManager.player.Steer.ReadValue<Vector2>().x;
            steer.y = InputManager.player.Steer.ReadValue<Vector2>().y;
            steer.z = InputManager.player.Ascend_Descend.ReadValue<float>();

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

            if (InputManager.player.Boost.IsPressed())
            {
                SetBoost(true);
            }
            else
            {
                SetBoost(false);
            }

            //Shooting
            if (InputManager.player.Shoot.IsPressed())
            {
                if (rangedWeapon == Weapon.RAVER_LAZER)
                {
                    UpdateLazer();
                }
                else if (rangedWeapon == Weapon.CHARGE_BOMB)
                {
                    chargeAmount += chargeSpeed * Time.deltaTime;
                    chargeMaterial.SetColor("_Color", Color.Lerp(Color.green * 2, Color.red * 2, chargeAmount / chargeSpeed));
                }
                else if(rangedWeapon == Weapon.MULTI_SHOT)
                {
                    for(int i = 0; i < targets.Count; i++)
                    {
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
        }
    }
    void OnDestroy()
    {
        InputManager.player.Shoot.performed -= Shoot_performed;
        InputManager.player.Shoot.canceled -= Shoot_canceled;
        InputManager.player.CenterCrosshair.performed -= CenterCrosshair;
        InputManager.player.ToggleStrafeMode.performed -= StrafeMode_performed;
        InputManager.player.Roll.performed -= Roll_performed;
        InputManager.player.ToggleWeapon.performed -= ToggleWeapon_performed;
    }
        
    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (rangedWeapon == Weapon.CHARGE_BOMB)
        {
            chargeAmount = 0;
            chargeEffect.gameObject.SetActive(true);
        }
        else if(rangedWeapon == Weapon.RAVER_LAZER)
        {
            FireLazer();
        }
    }
    
    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (rangedWeapon == Weapon.CHARGE_BOMB)
        {
            chargeEffect.gameObject.SetActive(false);
            FireChargeShot();
        }
        else if (rangedWeapon == Weapon.RAVER_LAZER)
        {
            if (lazer)
            {
                lazer.GetComponent<Lazer>().DeSpawn();
                lazer = null;
            }
        }
        else if (rangedWeapon == Weapon.MULTI_SHOT)
        {
            int targetCount = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].followTarget)
                {
                    targetCount++;
                }
            }

            if(targetCount > 0)
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
                    if(camera.boostEffect.isPlaying) camera.boostEffect.Stop();
                    thrustSpeed = baseSpeed;
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
        evadeTimer = 0;
        evading = true;
        if (evadeDirection == 1) evadeDirection = -1;
        else evadeDirection = 1;
    }
    
    private void ToggleWeapon_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //Cancel Current Weapon
        if (rangedWeapon == Weapon.CHARGE_BOMB)
        {
            chargeEffect.gameObject.SetActive(false);
        }
        else if (rangedWeapon == Weapon.RAVER_LAZER)
        {
            if (lazer)
            {
                lazer.GetComponent<Lazer>().DeSpawn();
                lazer = null;
            }
        }
        else if (rangedWeapon == Weapon.MULTI_SHOT)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].followTarget = null;
                targets[i].ui.SetActive(false);
            }

        }

        //Then Change Weapon
        float v = obj.ReadValue<float>();
        if (v > 0)
        {
            if(rangedWeaponIndex < 2)
            {
                rangedWeaponIndex++;
            }
            else
            {
                rangedWeaponIndex = 0;
            }
        }
        else if(v < 0)
        {
            if(rangedWeaponIndex > 0)
            {
                rangedWeaponIndex--;
            }
            else
            {
                rangedWeaponIndex = 2;
            }

        }
        rangedWeapon = (Weapon)rangedWeaponIndex;
        weaponText.text = " Weapon: " + rangedWeapon.ToString();
    }

    public void Teleport(Vector3 position)
    {
        controller.enabled = false;
        transform.position = position;
        camera.transform.position = position + (transform.up * 3) - (camera.transform.forward * camera.distanceFromPlayer);
        controller.enabled = true;
    }

    public void SetStrafeMode(bool active)
    {
        strafeMode = active;
        if(strafeMode)
        {
            thrustSpeed = baseSpeed;
            reticlePosition = new Vector2(Screen.width / 2, Screen.height / 2);
            reticle.rectTransform.position = reticlePosition;
            camera.transform.parent = transform;
        }
        else
        {
            thrustSpeed = baseSpeed;
            camera.transform.parent = transform.parent;
        }
    }

    public void SetBoost(bool active)
    {
        boosting = active;
        if(boosting)
        {
            if(strafeMode)
            {
                if(InputManager.player.Steer.IsPressed() || InputManager.player.Ascend_Descend.IsPressed())
                {
                    if (!camera.boostEffect.isPlaying) camera.boostEffect.Play();
                    thrustSpeed = boostSpeed;
                    thrustScale = boostThrusterScale;
                }
                else
                {
                    if (camera.boostEffect.isPlaying) camera.boostEffect.Stop();
                    thrustSpeed = baseSpeed;
                    thrustScale = normalThrusterScale;
                }
            }
            else
            {
                if (!camera.boostEffect.isPlaying) camera.boostEffect.Play();
                thrustSpeed = boostSpeed;
                thrustScale = boostThrusterScale;
            }
        }
        else
        {
            if (camera.boostEffect.isPlaying) camera.boostEffect.Stop();
            thrustSpeed = baseSpeed;
            thrustScale = normalThrusterScale;
        }
    }
    
    void OnRailsControls()
    {
        OnRailsFollowTarget.transform.position += OnRailsFollowTarget.transform.forward * speed * Time.deltaTime;


        offset += new Vector3(steer.x, steer.y, 0) * Time.deltaTime;
        offset.x = Mathf.Clamp01(offset.x);
        offset.y = Mathf.Clamp01(offset.y);
        offset.z = speed / 4;
       

        Vector3 offsetWorld = Camera.main.ViewportToWorldPoint(offset);
        transform.position = offsetWorld;

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
        controller.Move(((transform.forward * steer.y) + (transform.right * steer.x) + (transform.up * steer.z)).normalized * speed * Time.deltaTime);

        //Aiming
        float lookX = InputManager.player.Aim.ReadValue<Vector2>().x;
        float lookY = InputManager.player.Aim.ReadValue<Vector2>().y;
        transform.rotation *= Quaternion.AngleAxis(lookX * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookY * turnSpeed * Time.deltaTime, Vector3.right);
    }

    void AllRangeControls()
    {
        //Forward Movement
        controller.Move(transform.forward * speed * Time.deltaTime);

        transform.rotation *= Quaternion.AngleAxis(steer.x * turnSpeed * Time.deltaTime, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(steer.y * turnSpeed * Time.deltaTime, Vector3.right);

        //Auto Level
        if (steer.magnitude == 0)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(transform.localEulerAngles.z, 0, 5 * Time.deltaTime));
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
        if (obj)
        {
            Bullet b = obj.GetComponent<Bullet>();
            //Set Needed Variables
            b.damage = baseFirePower;
            b.owner = mesh.gameObject;
            b.explosive = explodingBullets;
            b.blastRadius = blastRadius;
            b.trail.material.SetColor("_Color", Color.white * 2);
        
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
        for(int i = 0; i < targets.Count; i++)
        {
            if(targets[i].followTarget && targets[i].ui.activeSelf)
            {
                GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                Bullet b = obj.GetComponent<Bullet>();
                //Set Needed Variables
                b.damage = baseFirePower;
                b.owner = mesh.gameObject;
                b.explosive = explodingBullets;
                b.blastRadius = blastRadius;
                b.trail.material.SetColor("_Color", Color.white * 2);
                b.homingTarget = targets[i].followTarget;
                targets[i].followTarget = null;
                targets[i].ui.SetActive(false);
            }
        }
    }

    void FireChargeShot()
    {
        //Initialize Bullet
        GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
        if (obj != null)
        {
            Bullet b = obj.GetComponent<Bullet>();
            b.trail.material.SetColor("_Color", chargeMaterial.GetColor("_Color"));
            b.owner = mesh.gameObject;
            
            if(chargeAmount >= 1)
            {
                b.explosive = true;
                b.damage = baseFirePower * missileMult;
                b.blastRadius = blastRadius;
            }
            else
            {
                b.damage = baseFirePower;
                b.explosive = false;
                b.blastRadius = 0;
            }

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

    void FireLazer()
    {
        if (!lazer)
        {
            lazer = GameManager.Get().objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
            lazer.owner = mesh.gameObject;
            lazer.damage = baseFirePower;
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
