using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazer : MonoBehaviour
{
    [ColorUsage(true, true)][SerializeField] Color[] colors;
    LineRenderer renderer;
    int colorIndex = 0;
    float colorChangeInterval = 0.05f;
    float colorChangeTimer = 0;

    public GameObject owner = null;
    public float damage = 5;
    public Vector3 direction;

    public float damageInterval = 0.1f;
    float damageTimer = 0;
    ObjectPool explosionPool;

    void OnEnable()
    {
        renderer = GetComponent<LineRenderer>();
        explosionPool = GameObject.Find("ExplosionPool").GetComponent<ObjectPool>();
        renderer.sharedMaterial.color = colors[colorIndex];
    }

    void Update()
    {
        Vector3 origin = owner.transform.position + owner.transform.forward;
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
            damageTimer = damageInterval;
        }
    }

    void CheckCollisions()
    {
        if(Physics.Linecast(renderer.GetPosition(0), renderer.GetPosition(1), out RaycastHit hit))
        {
            if(hit.transform.gameObject != owner)
            {
                if (hit.transform.tag == "Asteroid")
                {
                    Asteroid asteroid = hit.transform.GetComponent<Asteroid>();
                    explosionPool.Spawn(hit.point);
                    if (asteroid)
                    {
                        asteroid.RemoveBlock(hit.point);
                    }
                }
                else if (hit.transform.tag == "Planet")
                {
                    explosionPool.Spawn(hit.point);
                }
                else if (hit.transform.tag == "Enemy")
                {
                    HealthSystem health = hit.transform.GetComponent<HealthSystem>();
                    if (health)
                    {
                        health.TakeDamage(damage);
                    }
                }
                else if (hit.transform.tag == "Player")
                {
                    PlayerShip player = hit.transform.GetComponent<PlayerShip>();
                    if (player)
                    {
                        HealthSystem health = hit.transform.GetComponent<HealthSystem>();
                        if (health)
                        {
                            health.TakeDamage(damage);
                            if (health.IsDead())
                            {
                                explosionPool.Spawn(hit.transform.position);
                                hit.transform.gameObject.SetActive(false);
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
