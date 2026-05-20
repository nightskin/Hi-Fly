using UnityEngine;

public class Lazer : MonoBehaviour
{
    [ColorUsage(true, true)][SerializeField] Color[] colors;
    [SerializeField] LineRenderer renderer;
    [SerializeField][Min(0)] float damageInterval = 0.05f;
    int colorIndex = 0;
    [SerializeField][Min(1)] float colorChangeRate = 2.0f;
    float colorChangeTimer = 0;

    [HideInInspector] public GameObject owner = null;
    public int damage = 5;
    public float speed = 0.01f;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public Vector3 origin;
    [HideInInspector] public bool endFire;

    float collisionTimer;
    float length;
    float t;

    void OnEnable()
    {
        renderer.startWidth = 0.5f;
        renderer.endWidth = 1;
        t = 0;
        length = 0;
        collisionTimer = 0;
        renderer.sharedMaterial.color = colors[colorIndex];
        endFire = false;
    }

    void Update()
    {
        if (!GameManager.Get().gamePaused)
        {
            if(t < 1) t += speed * Time.deltaTime;
            length = Mathf.Lerp(length, Camera.main.farClipPlane, t);
            origin = owner.transform.position + owner.transform.forward;
            renderer.SetPosition(0, origin);
            renderer.SetPosition(1, origin + (direction * length));
            

            if(endFire)
            {
                renderer.startWidth = Mathf.Lerp(renderer.startWidth, 0, 10 * Time.fixedDeltaTime);
                renderer.endWidth = Mathf.Lerp(renderer.endWidth, 0, 10 * Time.fixedDeltaTime);
                if(renderer.endWidth <= 0)
                {
                    gameObject.SetActive(false);
                }
            }

            if (colorChangeTimer < 1)
            {
                colorChangeTimer += colorChangeRate * Time.deltaTime;
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
            renderer.sharedMaterial.SetColor("_Color", Color.Lerp(renderer.sharedMaterial.GetColor("_Color"), colors[colorIndex], colorChangeTimer));

            if (collisionTimer <= 0 && !endFire)
            {
                CheckCollisions();
                collisionTimer = damageInterval;
            }
            else
            {
                collisionTimer -= Time.deltaTime;
            }

        }
    }
    
    void CheckCollisions()
    {
        if (Physics.Linecast(origin, origin + (direction * length), out RaycastHit rayHit))
        {
            if (rayHit.transform.gameObject != owner)
            {
                if (rayHit.transform.tag == "Destructible")
                {
                    GameManager.Get().objectPool.Spawn("explosion", rayHit.point);
                    Asteroid asteroid = rayHit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(rayHit);
                        return;
                    }

                    PlanetChunk planet = rayHit.transform.GetComponent<PlanetChunk>();
                    if(planet)
                    {
                        planet.RemoveBlock(rayHit);
                        return;
                    }

                    TerrainChunk terrain = rayHit.transform.GetComponent<TerrainChunk>();
                    if (terrain)
                    {
                        terrain.TeraForm(rayHit, 0.1f);
                        return;
                    }
                }
                else if (rayHit.transform.tag == "Surface")
                {
                    GameManager.Get().objectPool.Spawn("explosion", rayHit.point);
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
                                GameManager.Get().objectPool.Spawn("explosion", rayHit.point);
                                rayHit.transform.gameObject.SetActive(false);
                                GameManager.Get().gameOver = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
