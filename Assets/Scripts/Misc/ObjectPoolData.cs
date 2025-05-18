using UnityEngine;

[System.Serializable]
public class ObjectPoolData
{
    public string name;
    public GameObject prefab;
    [Min(1)] public int size = 100;
}
