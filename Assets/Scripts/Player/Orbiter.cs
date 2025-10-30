using UnityEngine;

public class Orbiter : MonoBehaviour
{
    [SerializeField] Transform bulletSpawn;
    [SerializeField][Range(0, 0.999f)] float threshold = 0;

    GameObject target;
    Lazer lazer;
    public enum Type
    {
        TURRET,
        MISSILE,
        RAVER_LAZER,
    }
    public Type type;

    int damage;
    float fireRate;
    float shootTimer = 1;

    void OnEnable()
    {
        target = GameManager.Get().objectPool.GetActiveFromObjectPool("enemy");
        if (type == Type.TURRET)
        {
            fireRate = 1;
            damage = 5;
        }
        else if (type == Type.MISSILE)
        {
            fireRate = 0.5f;
            damage = 15;
        }
        else if (type == Type.RAVER_LAZER)
        {
            fireRate = 2;
            damage = 2;
        }
    }
    
    public void Fire()
    {
        if(!target)
        {
            target = GameManager.Get().objectPool.GetActiveFromObjectPool("enemy");
        }

        if(type != Type.RAVER_LAZER)
        {
            if (shootTimer <= 0)
            {
                Vector3 heading = (target.transform.position - bulletSpawn.position).normalized;
                float dot = Vector3.Dot(bulletSpawn.forward, heading);
                if (dot > threshold)
                {
                    if (type == Type.TURRET)
                    {
                        var obj = GameManager.Get().objectPool.SpawnFromObjectPool("bullet", bulletSpawn.position);
                        var p = obj.GetComponent<Bullet>();
                        p.damage = damage;
                        p.owner = GameManager.Get().playerShip.mesh.gameObject;
                        p.direction = heading;
                    }
                    else if (type == Type.MISSILE)
                    {
                        var obj = GameManager.Get().objectPool.SpawnFromObjectPool("missile", bulletSpawn.position);
                        var p = obj.GetComponent<Missile>();
                        p.owner = GameManager.Get().playerShip.mesh.gameObject;
                        p.damage = damage;
                        p.direction = heading;
                    }
                }
            }
            else
            {
                shootTimer -= fireRate * Time.deltaTime;
            }
        }
        else
        {
            Vector3 heading = (target.transform.position - bulletSpawn.position).normalized;
            float dot = Vector3.Dot(bulletSpawn.forward, heading);

            if (dot > threshold)
            {
                if (lazer)
                {
                    lazer.gameObject.SetActive(true);
                    lazer.origin = bulletSpawn.position;
                    lazer.direction = Vector3.Lerp(lazer.direction, heading, 10 * Time.deltaTime);
                }
                else
                {
                    lazer = GameManager.Get().objectPool.SpawnFromObjectPool("lazer", bulletSpawn.position).GetComponent<Lazer>();
                    lazer.damage = damage;
                    lazer.origin = bulletSpawn.position;
                    lazer.direction = bulletSpawn.forward;
                    lazer.owner = GameManager.Get().playerShip.mesh.gameObject;
                }
            }
            else
            {
                if (lazer)
                {
                    lazer.gameObject.SetActive(false);
                }
            }
        }
    }
}
