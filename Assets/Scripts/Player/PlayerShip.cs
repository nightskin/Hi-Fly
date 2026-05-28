using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;

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
    public PlayerCamera playerCamera;
    [SerializeField] GameObject boostEffect;
    [SerializeField] GameObject lockUI;
    [SerializeField] Transform hud;
    [SerializeField] Material chargeMaterial;
    [SerializeField] ParticleSystem chargeEffect;
    [SerializeField] GameObject[] trails;
    [SerializeField] Transform bulletSpawn;
    [SerializeField] Transform mesh;
    [SerializeField] CharacterController controller;

    //Flight variables
    float speed;
    float targetSpeed;
    Vector3 moveInput;
    Vector2 reticlePosition;
    [HideInInspector] public bool forwardBoost;
    
    [SerializeField][Min(1)] float turnSpeed = 100;
    [SerializeField] float baseSpeed = 50;
    [SerializeField] float boostSpeed = 200;
    [SerializeField] float thrustSpeed = 250;
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
        InputManager.player.Boost.performed += Boost_pressed;
        InputManager.player.Boost.canceled += Boost_released;
        InputManager.player.ToggleWeapon.performed += ToggleWeapon_pressed;
        InputManager.player.Shoot.performed += Shoot_pressed;
        InputManager.player.Shoot.canceled += Shoot_released;
        InputManager.player.CenterReticle.performed += CenterReticle_pressed;

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
        playerCamera.transform.parent = transform;
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
            if (forwardBoost) ThrustControls();
            else StrafeControls();

            //Auto Level
            if (moveInput.magnitude == 0 && InputManager.player.Aim.ReadValue<Vector2>().magnitude == 0 &&  transform.localEulerAngles.z != 0)
            {
                transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, Mathf.LerpAngle(transform.localEulerAngles.z, 0, 5 * Time.deltaTime));
            }

            //veering left and right
            float turnX = InputManager.player.Steer.ReadValue<Vector2>().x * -45;
            mesh.localEulerAngles = new Vector3(0,0,turnX);

            //Handles Lazer
            if(InputManager.player.Shoot.IsPressed() && equipedWeapon == Weapon.LAZER)
            {
                UpdateLazer();
            }
        }
    }
    void OnDestroy()
    {
        InputManager.player.Boost.performed -= Boost_pressed;
        InputManager.player.Boost.canceled -= Boost_released;
        InputManager.player.ToggleWeapon.performed -= ToggleWeapon_pressed;
        InputManager.player.Shoot.performed -= Shoot_pressed;
        InputManager.player.Shoot.canceled -= Shoot_released;
        InputManager.player.CenterReticle.performed -= CenterReticle_pressed;
    }


    private void Boost_pressed(InputAction.CallbackContext obj)
    {
        targetSpeed = boostSpeed;
        if(moveInput.magnitude <= 0.1f)
        {
            forwardBoost = true;
            playerCamera.transform.parent = transform.parent;
            boostEffect.SetActive(true);
            for(int i = 0; i < trails.Length; i++)
            {
                trails[i].gameObject.SetActive(true);
            }
        }
    }

    private void Boost_released(InputAction.CallbackContext obj)
    {
        targetSpeed = baseSpeed;
        forwardBoost = false;
        playerCamera.transform.parent = transform;
        boostEffect.SetActive(false);
        reticlePosition = Vector2.zero;
        reticle.rectTransform.anchoredPosition = reticlePosition;
        for(int i = 0; i < trails.Length; i++)
        {
            trails[i].gameObject.SetActive(false);
        }
    }
    
    private void Shoot_pressed(InputAction.CallbackContext obj)
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

    private void Shoot_released(InputAction.CallbackContext obj)
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
            int targetCount = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].followTarget)
                {
                    targetCount++;
                }
            }

            if (targetCount > 0)
            {
                StartCoroutine(FireMultiBlaster());
            }
            else
            {
                FireBlaster();
            }
        }
    }
    
    private void ToggleWeapon_pressed(InputAction.CallbackContext obj)
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

    private void CenterReticle_pressed(InputAction.CallbackContext obj)
    {
        reticlePosition = Vector2.zero;
        reticle.rectTransform.anchoredPosition = reticlePosition;
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

        controller.Move(((transform.forward * moveInput.y) + (transform.right * moveInput.x) + (transform.up * moveInput.z)).normalized * speed * Time.deltaTime);

        //Aiming
        float lookX = InputManager.player.Aim.ReadValue<Vector2>().x;
        float lookY = InputManager.player.Aim.ReadValue<Vector2>().y;
        transform.rotation *= Quaternion.AngleAxis(lookX, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(-lookY, Vector3.right);
    }

    void ThrustControls()
    {
        //Forward Movement
        moveInput = InputManager.player.Steer.ReadValue<Vector2>().normalized;
        controller.Move(transform.forward * speed * Time.deltaTime);
        
        //Turning
        transform.rotation *= Quaternion.AngleAxis(moveInput.x, Vector3.up);
        transform.rotation *= Quaternion.AngleAxis(moveInput.y, Vector3.right);


        //reticle movement
        Vector2 reticleInput = InputManager.player.Aim.ReadValue<Vector2>();
        reticlePosition += reticleInput * GameSettings.aimSensitivy * Time.deltaTime;
        reticlePosition.x = Mathf.Clamp(reticlePosition.x,-400,400);
        reticlePosition.y = Mathf.Clamp(reticlePosition.y,-183,183);
        reticle.rectTransform.anchoredPosition = reticlePosition;
    }

    void FireBlaster()
    {
        //Initialize Bullet
        GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
        if (obj)
        {
            Bullet b = obj.GetComponent<Bullet>();
            b.owner = gameObject;

            if(chargeAmount >= 1)
            {
                b.explosive = true;
                b.damage = baseFirePower;
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

    int GetNumberOfActiveTargets()
    {
        int n = 0;
        foreach(var t in targets)
        {
            if(t.followTarget != null) n++;
        }
        return n;
    }

    IEnumerator FireMultiBlaster()
    {
        int numberOfTargets = GetNumberOfActiveTargets();
        for(int i = 0; i < targets.Count; i++)
        {
            if(targets[i].followTarget && targets[i].ui.activeSelf)
            {
                GameObject obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                Bullet b = obj.GetComponent<Bullet>();
                //Set Needed Variables
                b.owner = gameObject;

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
                b.homingTarget = targets[i].followTarget;
                targets[i].followTarget = null;
                targets[i].ui.SetActive(false);
                yield return new WaitForSeconds(0.05f);
            }
        }
        chargeAmount = 0;
    }

    void FireLazer()
    {
        if (!lazer)
        {
            lazer = GameManager.Get().objectPool.Spawn("lazer", Vector3.zero).GetComponent<Lazer>();
            lazer.owner = gameObject;
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
