using UnityEngine;

public class TitleScreen : MonoBehaviour
{

    void Update()
    {
        if(Input.anyKeyDown)
        {
            StartCoroutine(SceneLoader.instance.LoadLevel("Menu"));
        }
    }

}
