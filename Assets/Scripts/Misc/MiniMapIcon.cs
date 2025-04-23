using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    public Transform followTarget;


    void Update()
    {
        if(followTarget)
        {
            transform.position = followTarget.position;
            transform.localRotation = Quaternion.Euler(90, followTarget.transform.localEulerAngles.y ,0);
        }
    }
}
