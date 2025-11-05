using UnityEngine;

public class PlayerTurret : MonoBehaviour
{
    [SerializeField] Transform bulletSpawn;
    [SerializeField][Range(-1, 1)] float threshold = 0.25f;

    Transform target;
    Lazer lazer;

    public PlayerShip.RangedWeapon type;

    [HideInInspector] public int damage;
    [HideInInspector] public float blastRadius;
    float fireRate;
    float shootTimer = 1;

    void OnEnable()
    {
        target = FindTarget();
        if (type == PlayerShip.RangedWeapon.MULTI_SHOT)
        {
            fireRate = 1;
            damage = 5;
        }
        else if (type == PlayerShip.RangedWeapon.CHARGE_BOMB)
        {
            fireRate = 0.5f;
            damage = 15;
        }
        else if (type == PlayerShip.RangedWeapon.RAVER_LAZER)
        {
            fireRate = 2;
            damage = 2;
        }
    }
    
    public void Fire()
    {
        target = FindTarget();

        if (shootTimer <= 0)
        {
            if(target)
            {
                Vector3 heading = (target.position - bulletSpawn.position).normalized;
                if (type == PlayerShip.RangedWeapon.MULTI_SHOT)
                {
                    var obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                    var p = obj.GetComponent<Bullet>();
                    p.explosive = false;
                    p.blastRadius = 0;
                    p.damage = damage;
                    p.owner = GameManager.Get().playerShip.mesh.gameObject;
                    p.direction = heading;
                }
                else if (type == PlayerShip.RangedWeapon.CHARGE_BOMB)
                {
                    var obj = GameManager.Get().objectPool.Spawn("bullet", bulletSpawn.position);
                    var p = obj.GetComponent<Bullet>();
                    p.owner = GameManager.Get().playerShip.mesh.gameObject;
                    p.explosive = true;
                    p.blastRadius = blastRadius;
                    p.damage = damage;
                    p.direction = heading;
                }
                shootTimer = 1;
            }
        }
        else
        {
            shootTimer -= fireRate * Time.fixedDeltaTime;
        }
    }
    
    Transform FindTarget()
    {
        var targets = GameManager.Get().objectPool.GetObjectPool("enemy");
        for (int i = 0; i < targets.childCount; i++)
        {
            if(targets.GetChild(i).gameObject.activeSelf)
            {
                Vector3 heading = (targets.GetChild(i).position - Camera.main.transform.position).normalized;
                float dot = Vector3.Dot(Camera.main.transform.forward, heading);
                if (dot >= threshold)
                {
                    return targets.GetChild(i);
                }
            }
        }
        return null;
    }
}
