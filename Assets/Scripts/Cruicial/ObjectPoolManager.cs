using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] ObjectPoolData[] poolData;

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

    public GameObject SpawnFromObjectPool(string objectPoolName, Vector3 position)
    {
        Transform pool = transform.Find(objectPoolName);
        if (pool) 
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
    
    public GameObject GetActiveFromObjectPool(string objectPoolName)
    {
        Transform pool = transform.Find(objectPoolName);
        if (pool)
        {
            for (int i = 0; i < pool.childCount; i++)
            {
                if (pool.GetChild(i).gameObject.activeSelf)
                {
                    return pool.GetChild(i).gameObject;
                }
            }
            Debug.Log("No Active Objects Found");
            return null;
        }
        Debug.Log("Could Not Find Object Pool");
        return null;
    }
}
