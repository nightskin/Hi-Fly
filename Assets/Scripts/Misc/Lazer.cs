using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    [ColorUsage(true, true)][SerializeField] Color[] colors;
    [SerializeField] LineRenderer renderer;
    int colorIndex = 0;
    float colorChangeInterval = 0.05f;
    float colorChangeTimer = 0;

    [HideInInspector] public GameObject owner = null;
    public float damage = 5;
    [HideInInspector] public Vector3 direction;
    [HideInInspector] public Vector3 origin;

    public float collisionInterval = 0.1f;
    float damageTimer = 0;
    ObjectPool objectPool;
    
    void OnEnable()
    {
        objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPool>();
        renderer.sharedMaterial.color = colors[colorIndex];
    }

    void Update()
    {
        if(!GameManager.gamePaused)
        {
            origin = owner.transform.position + owner.transform.forward;
            renderer.SetPosition(0, origin);
            renderer.SetPosition(1, origin + (direction * Camera.main.farClipPlane));

            colorChangeTimer -= Time.deltaTime;
            if (colorChangeTimer <= 0)
            {
                ChangeColor();
                colorChangeTimer = colorChangeInterval;
            }

            damageTimer -= Time.deltaTime;
            if(damageTimer <= 0)
            {
                CheckCollisions();
                damageTimer = collisionInterval;
            }
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
                    Asteroid asteroid = rayHit.transform.GetComponent<Asteroid>();
                    objectPool.Spawn("explosion", rayHit.point);
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(rayHit);
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

    void ChangeColor()
    {
        if(colorIndex < colors.Length - 1)
        {
            colorIndex++;
        }
        else
        {
            colorIndex = 0;
        }
        renderer.sharedMaterial.color = colors[colorIndex];
    }

    public void DeSpawn()
    {
        gameObject.SetActive(false);
    }
}
