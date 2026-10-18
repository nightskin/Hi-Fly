using UnityEngine;

public class Lazer : MonoBehaviour
{
    [SerializeField] LineRenderer renderer;
    [SerializeField][Min(0)] float damageInterval = 0.05f;

    [HideInInspector] public GameObject owner = null;
    public int power = 5;
    public float speed = 0.01f;
    [HideInInspector] public Vector3 origin;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public bool endFire;

    float damageTimer = 0;
    float length;
    float t;

    void OnEnable()
    {
        renderer.startWidth = 0.5f;
        renderer.endWidth = 1;
        t = 0;
        length = 0;
        damageTimer = 0;
        endFire = false;
    }

    void Update()
    {
        if(t < 1) t += speed * Time.fixedDeltaTime;
        length = Mathf.Lerp(length, Camera.main.farClipPlane, t);
        origin = owner.transform.position + owner.transform.forward;
        renderer.SetPosition(0, origin);
        renderer.SetPosition(1, origin + (direction * length));
    }

    void FixedUpdate()
    {
        if (GameManager.Get().state == GameManager.GameState.GAME_PLAY)
        {
            if(endFire)
            {
                renderer.startWidth = Mathf.Lerp(renderer.startWidth, 0, 10 * Time.fixedDeltaTime);
                renderer.endWidth = Mathf.Lerp(renderer.endWidth, 0, 10 * Time.fixedDeltaTime);
                if(renderer.endWidth <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                CheckCollisions();
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
                    if(damageTimer <= 0)
                    {
                        GameManager.Get().objectPool.Spawn("sparks", rayHit.point);
                        damageTimer = damageInterval;
                    }
                    else
                    {
                        damageTimer -= Time.fixedDeltaTime;
                    }

                    Asteroid asteroid = rayHit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.Teraform(rayHit);
                    }

                    Planet planet = rayHit.transform.GetComponent<Planet>();
                    if(planet)
                    {
                        planet.Teraform(rayHit);
                    }

                    TerrainChunk terrain = rayHit.transform.GetComponent<TerrainChunk>();
                    if (terrain)
                    {
                        terrain.TeraForm(rayHit, 0.1f);
                    }
                }
                else if (rayHit.transform.tag == "Enemy" || rayHit.transform.tag == "Player")
                {
                    if(damageTimer <= 0)
                    {
                        GameManager.Get().objectPool.Spawn("sparks", rayHit.point);
                        HealthSystem health = rayHit.transform.GetComponent<HealthSystem>();
                        if (health)
                        {
                            health.TakeDamage(power);
                        }
                        damageTimer = damageInterval;
                    }
                    else
                    {
                        damageTimer -= Time.fixedDeltaTime;
                    }
                }
            }
        }
    }
}
