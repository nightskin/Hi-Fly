using UnityEngine;

public class DebugTerrain : MonoBehaviour
{
    public float min = 0;
    public float max = 0;



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position + new Vector3(0, min, 0), new Vector3(100,1 ,100));
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + new Vector3(0, max, 0), new Vector3(100, 1, 100));
    }


}
