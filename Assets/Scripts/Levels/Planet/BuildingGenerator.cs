using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class BuildingGenerator : MonoBehaviour
{
    public int resolution = 100;
    public string seed = "";
    public float spacing = 50;
    public List<Vector3> landPoints;

    [Min(1)] public float minHeight = 50;
    [Min(1)] public float maxHeight = 200;
    
    Noise noise;

    void MakeBuilding(Vector3 position, float height)
    {
        
    }
    
    
    public void Generate()
    {
        
    }
    
    void Start()
    {
        
    }

}
