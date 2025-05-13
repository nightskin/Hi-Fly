using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniMapIcon : MonoBehaviour
{
    public Transform followTarget;

    void Start()
    {
        if(SceneManager.GetActiveScene().name != "Hub")
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if(followTarget)
        {
            transform.position = followTarget.position;
            transform.localRotation = Quaternion.Euler(90, followTarget.transform.localEulerAngles.y ,0);
        }
    }
}
