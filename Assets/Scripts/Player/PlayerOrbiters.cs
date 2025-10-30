using UnityEngine;

public class PlayerOrbiters : MonoBehaviour
{
    [SerializeField] PlayerShip playerShip;
    [SerializeField] GameObject orbiterPrefab;
    [SerializeField] float orbiterRadius = 3;

    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis(45 * Time.deltaTime, Vector3.forward);
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
            transform.GetChild(i).localEulerAngles = new Vector3(0, 0, d);
        }
    }

    public void FireOrbiters()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Orbiter o = transform.GetChild(i).GetComponent<Orbiter>();
            o.Fire();
        }
    }

}
