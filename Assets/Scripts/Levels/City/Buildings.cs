using System.Collections.Generic;
using UnityEngine;


public class Buildings : MonoBehaviour
{
    [SerializeField] List<GameObject> buildingPrefabs;
    [SerializeField] bool addCollisions = true;
    [SerializeField][Min(1)] float groundSurfaceArea = 10000;
    [SerializeField][Min(1)] float buildingSpacing = 100;
    [SerializeField][Min(2)] float maxBuildingSize = 75;
    [SerializeField][Min(1)] float minBuildingSize = 50;
    
    int numberOfbuildings;

    void Awake()
    {
        //Setup
        numberOfbuildings = (int)groundSurfaceArea / (int)buildingSpacing;
        for(int x = 0; x < numberOfbuildings; x++)
        {
            for(int z = 0; z < numberOfbuildings; z++)
            {
                int make = Mathf.RoundToInt(Random.value);
                if(make == 1)
                {
                    float size = Random.Range(minBuildingSize, maxBuildingSize);
                    MakeBuilding(new Vector3(x, 0, z) * buildingSpacing, new Vector3(size, size, size), addCollisions);
                }
            }
        }
    }

    void MakeBuilding(Vector3 position, Vector3 size, bool collider = true)
    {
        if (buildingPrefabs.Count > 0)
        {
            int i = Random.Range(0, buildingPrefabs.Count);
            var b = Instantiate(buildingPrefabs[i], position, Quaternion.identity, transform);
            b.transform.localScale = size;
            b.tag = "Planet";
            b.isStatic = true;
            if(collider)
            {
                b.AddComponent<MeshCollider>();
            }

        }
    }
}
