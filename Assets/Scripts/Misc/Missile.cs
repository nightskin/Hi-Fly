using UnityEngine;

public class Missile : MonoBehaviour
{
    public float lifetime = 5;
    public GameObject owner = null;
    public Transform homingTarget = null;
    public float speed = 1000;
    public Vector3 direction;
    public int damage;
    public float blastRadius = 20;

    BoxCollider box;
    AudioSource sfx;
    TrailRenderer trail;
    Vector3 prevPosition;
    bool hit;
    float life = 0;
    

    void Awake()
    {
        life = lifetime;
        box = GetComponent<BoxCollider>();
        sfx = GetComponent<AudioSource>();
        trail = transform.Find("Trail").GetComponent<TrailRenderer>();
    }

    void OnEnable()
    {
        hit = false;
        sfx.Play();
        homingTarget = null;
        direction = Vector3.zero;
        trail.Clear();
        trail.emitting = true;
        prevPosition = transform.position;
        life = lifetime;
    }
    
    void Update()
    {
        if (!GameManager.Get().gamePaused)
        {
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                transform.position = Vector3.MoveTowards(transform.position, homingTarget.transform.position, speed * Time.deltaTime);
                if(Vector3.Distance(transform.position, homingTarget.position) < 1.0f)
                {
                    GameManager.Get().objectPool.Spawn("powerBombExplosion", transform.position);
                    DeSpawn();
                }
            }
            else
            {
                transform.position += direction * speed * Time.deltaTime;
            }

            //If bullet has not hit something check collisions
            if (!hit)
            {
                CheckCollisions();
            }
            else
            {
                trail.emitting = false;
                if (!sfx.isPlaying)
                {
                    DeSpawn();
                }
            }

            //Destroy Bullet After A Certain Time has Past
            if (life > 0)
            {
                life -= Time.deltaTime;
            }
            else
            {
                DeSpawn();
            }
        }
    }

    void CheckCollisions()
    {
        if (Physics.BoxCast(prevPosition, box.size, direction, out RaycastHit rayhit, transform.rotation, Vector3.Distance(prevPosition, transform.position)))
        {
            if (rayhit.transform.gameObject != owner)
            {
                var powerBombExplosion = GameManager.Get().objectPool.Spawn("powerBombExplosion", transform.position);
                powerBombExplosion.GetComponent<PowerBomb>().damage = damage;
                powerBombExplosion.GetComponent<PowerBomb>().blastRadius = blastRadius;

                if (rayhit.transform.tag == "Destructible")
                {
                    Asteroid asteroid = rayhit.transform.GetComponent<Asteroid>();
                    if (asteroid)
                    {
                        asteroid.RemoveBlocksInRadius(rayhit, blastRadius);
                        DeSpawn();
                    }
                    DestructibleTerrainChunk terrain = rayhit.transform.GetComponent<DestructibleTerrainChunk>();
                    if (terrain)
                    {
                        terrain.TeraForm(rayhit, 1);
                        DeSpawn();
                    }
                }
                else
                {
                    DeSpawn();
                }
            }
        }
    }
    
    void DeSpawn()
    {
        trail.emitting = false;
        gameObject.SetActive(false);
    }
}
