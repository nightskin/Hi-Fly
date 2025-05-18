using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public ObjectPoolData[] poolData;


    void Awake()
    {
        transform.position = Vector3.zero;
        foreach(ObjectPoolData data in poolData) 
        {
            GameObject poolObj = new GameObject();
            poolObj.isStatic = true;
            poolObj.name = data.name;
            poolObj.transform.parent = transform;

            for(int i = 0; i < data.size; i++) 
            {
                var obj = Instantiate(data.prefab, poolObj.transform);
                obj.SetActive(false);
            }
        }        
    }

    public GameObject Spawn(string name, Vector3 position)
    {
        Transform pool = transform.Find(name);
        if (pool != null) 
        {
            for(int i = 0; i < pool.childCount; i++) 
            {
                if(!pool.GetChild(i).gameObject.activeSelf)
                {
                    pool.GetChild(i).transform.position = position;
                    pool.GetChild(i).gameObject.SetActive(true);
                    return pool.GetChild(i).gameObject;
                }
            }
        }
        Debug.Log("Could Not Find Object Pool");
        return null;
    }

}
