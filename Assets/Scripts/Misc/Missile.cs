using UnityEngine;

public class Missile : MonoBehaviour
{
    public float lifetime = 5;
    public GameObject owner = null;
    public Transform homingTarget = null;
    public float speed = 1000;
    public Vector3 direction;

    BoxCollider box;
    AudioSource sfx;
    TrailRenderer trail;
    ObjectPool objectPool;
    Vector3 prevPosition;
    bool hit;
    float life = 0;
    

    void Awake()
    {
        life = lifetime;
        box = GetComponent<BoxCollider>();
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
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
        if (!GameManager.gamePaused)
        {
            //Basic Movement
            prevPosition = transform.position;
            if (homingTarget)
            {
                transform.position = Vector3.MoveTowards(transform.position, homingTarget.transform.position, speed * Time.deltaTime);
                if(Vector3.Distance(transform.position, homingTarget.position) < 1.0f)
                {
                    objectPool.Spawn("powerBombExplosion", transform.position);
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
                objectPool.Spawn("powerBombExplosion", transform.position);
                DeSpawn();
            }
        }
    }

    
    void DeSpawn()
    {
        trail.emitting = false;
        gameObject.SetActive(false);
    }
}
