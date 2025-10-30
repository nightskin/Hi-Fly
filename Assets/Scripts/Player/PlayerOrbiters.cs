using UnityEngine;

public class PlayerOrbiters : MonoBehaviour
{
    public ObjectPool objectPool;
    [SerializeField] PlayerShip playerShip;
    [SerializeField] GameObject orbiterPrefab;
    [SerializeField] float orbiterRadius = 3;


    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(45 * Time.deltaTime, Vector3.forward);
        //Just Here For Debug
        if (InputManager.input.Player.Fire2.WasPressedThisFrame())
        {
            AddOrbiter(Orbiter.Type.TURRET);
        }
    }

    void FixedUpdate()
    {
        FireOrbiters();
    }

    public void AddOrbiter(Orbiter.Type type)
    {
        var obj = Instantiate(orbiterPrefab, transform);
        obj.GetComponent<Orbiter>().type = type;

        for (int i = 0; i < transform.childCount; i++)
        {
            float fraction = 360 / transform.childCount;
            float d = i * fraction;            
            float r = d * Mathf.Deg2Rad;
            float x = orbiterRadius * Mathf.Sin(r);
            float y = orbiterRadius * Mathf.Cos(r);
            transform.GetChild(i).localPosition = new Vector3(x, y, 0);
        }
    }

    public void FireOrbiters()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Orbiter o = transform.GetChild(i).GetComponent<Orbiter>();
            o.shootTimer -= o.fireRate * Time.fixedDeltaTime;
            if(o.shootTimer <= 0)
            {
                o.Fire();
            }
        }
    }

}
