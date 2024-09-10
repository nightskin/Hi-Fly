using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject obj;
    [SerializeField][Min(1)] int count = 100;
    GameObject[] pool;

    void Awake()
    {
        pool = new GameObject[count];
        for(int i = 0; i < count; i++)
        {
            pool[i] = Instantiate(obj, transform);
            pool[i].SetActive(false);
        }
    }   
    
    public GameObject Spawn(Vector3 position)
    {
        foreach(GameObject obj in pool) 
        {
            if(!obj.activeSelf)
            {
                obj.transform.position = position;
                obj.SetActive(true);
                return obj;
            }
        }
        return null;
    }

}
