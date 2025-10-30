using UnityEngine;

public class Orbiter : MonoBehaviour
{
    [SerializeField] Transform bulletSpawn;
    PlayerOrbiters pivot;
    public enum Type
    {
        TURRET,
        MISSILE,
        BEAM_RIFLE,
    }
    public Type type;


    [HideInInspector]public float fireRate = 1.0f;
    [HideInInspector] public float shootTimer = 1;

    void Start()
    {
        pivot = transform.parent.GetComponent<PlayerOrbiters>();

        if (type == Type.TURRET)
        {
            fireRate = 2.0f;
        }
        else if(type == Type.MISSILE)
        {
            fireRate = 1.0f;
        }
        else if(type == Type.BEAM_RIFLE)
        {
            fireRate = 1.0f;
        }
    }

    public void Fire()
    {
        if(type == Type.TURRET)
        {

        }

        fireRate = 1.0f;
    }
}
