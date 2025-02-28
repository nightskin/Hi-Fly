using UnityEngine;

public class City : MonoBehaviour
{
    [Min(100)] public float maxElevation = 500;
    [SerializeField][Min(1)] float groundMeshSize = 10;

    [SerializeField] Transform bounds;
    [SerializeField] Transform playArea;

    void Awake()
    {
        GameManager.InitRandom();

        if(playArea && bounds)
        {
            bounds.transform.position = new Vector3(playArea.position.x, maxElevation / 2, playArea.position.z);
            bounds.transform.localScale = new Vector3(playArea.localScale.x * groundMeshSize, maxElevation, playArea.localScale.z * groundMeshSize);
        }
        else
        {
            Debug.Log("Set PlayArea And Bound Objects");
        }


    }

}
