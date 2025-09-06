using UnityEngine;

public class Lazer : MonoBehaviour
{
    [ColorUsage(true, true)][SerializeField] Color[] colors;
    [SerializeField] LineRenderer renderer;
    int colorIndex = 0;
    float colorChangeTimer = 0;

    [HideInInspector] public GameObject owner = null;
    public float damage = 5;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public Vector3 origin;
    ObjectPool objectPool;

    float collisionTimer;

    void OnEnable()
    {
        collisionTimer = 0;
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
        renderer.sharedMaterial.color = colors[colorIndex];
    }

    void Update()
    {
        if (!GameManager.gamePaused)
        {
            origin = owner.transform.position + owner.transform.forward;
            renderer.SetPosition(0, origin);
            renderer.SetPosition(1, origin + (direction * Camera.main.farClipPlane));


            if (colorChangeTimer < 1)
            {
                colorChangeTimer += Time.deltaTime;
            }
            else
            {
                if (colorIndex < colors.Length - 1)
                {
                    colorIndex++;
                }
                else
                {
                    colorIndex = 0;
                }
                colorChangeTimer = 0;
            }
            renderer.sharedMaterial.SetColor("_MainColor", Color.Lerp(renderer.sharedMaterial.GetColor("_MainColor"), colors[colorIndex], colorChangeTimer));

        }
    }

    void FixedUpdate()
    {
        if (collisionTimer <= 0)
        {
            CheckCollisions();
            collisionTimer = 0.1f;
        }
        else
        {
            collisionTimer -= Time.fixedDeltaTime;
        }
    }

    void CheckCollisions()
    {
        if (Physics.SphereCast(origin, renderer.startWidth * renderer.widthMultiplier, direction, out RaycastHit rayHit, Camera.main.farClipPlane))
        {
            if (rayHit.transform.gameObject != owner)
            {
                if (rayHit.transform.tag == "Destructible")
                {
                    objectPool.Spawn("explosion", rayHit.point);
                    Asteroid asteroid = rayHit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(rayHit);
                        return;
                    }
                    DestructibleTerrainChunk terrain = rayHit.transform.GetComponent<DestructibleTerrainChunk>();
                    if (terrain)
                    {
                        terrain.TeraForm(rayHit, 0.1f);
                        return;
                    }
                }
                else if (rayHit.transform.tag == "Surface")
                {
                    objectPool.Spawn("explosion", rayHit.point);
                }
                else if (rayHit.transform.tag == "Enemy")
                {
                    HealthSystem health = rayHit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(damage);
                    }
                }
                else if (rayHit.transform.tag == "Player")
                {
                    PlayerShip player = rayHit.transform.GetComponent<PlayerShip>();
                    if (player)
                    {
                        HealthSystem health = rayHit.transform.GetComponent<HealthSystem>();
                        if (health)
                        {
                            health.TakeDamage(damage);
                            if (health.IsDead())
                            {
                                objectPool.Spawn("explosion", rayHit.point);
                                rayHit.transform.gameObject.SetActive(false);
                                GameManager.gameOver = true;
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void DeSpawn()
    {
        gameObject.SetActive(false);
    }
}
