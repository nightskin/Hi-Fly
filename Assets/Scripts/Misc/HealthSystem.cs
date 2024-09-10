using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] Slider healthBar;
    [SerializeField] int maxHP = 100;
    [SerializeField] Material[] hitMaterial;
    
    int hp;
    bool flickering = false;
    float flickerTimer;
    float damageFlicker = 0.05f;

    MeshRenderer meshRenderer;
    Material[] defaultMaterial;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterial = meshRenderer.materials;
        
        hp = maxHP;
        if(healthBar)
        {
            healthBar.wholeNumbers = true;
            healthBar.maxValue = maxHP;
            healthBar.minValue = 0;
            healthBar.value = hp;
        }
    }

    void Update()
    {
        if(flickering)
        {
            if(flickerTimer > 0)
            {
                flickerTimer -= Time.deltaTime;
            }
            else
            {
                meshRenderer.materials = defaultMaterial;
                flickering = false;
            }
        }
    }
    
    public int GetHealth()
    {
        return hp;
    }

    public int GetMaxHealth()
    {
        return maxHP;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if(hp < 0) 
        {
            hp = 0;
        }

        if(healthBar)
        {
            healthBar.value = hp;
        }

        meshRenderer.materials = hitMaterial;        
        flickerTimer = damageFlicker;
        flickering = true;
    }

    public void Heal(int amount)
    {
        hp += amount;
        if(hp > maxHP)
        {
            hp = maxHP;
        }

        if (healthBar)
        {
            healthBar.value = hp;
        }
    }

    public bool HasBeenHitOnce()
    {
        return hp < maxHP;
    }

    public bool IsAlive()
    {
        return hp > 0;
    }

    public bool IsDead()
    {
        return hp <= 0;
    }
}
