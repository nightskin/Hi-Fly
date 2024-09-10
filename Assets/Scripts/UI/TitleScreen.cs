using UnityEngine;

public class TitleScreen : MonoBehaviour
{

    void Update()
    {
        if(Input.anyKeyDown)
        {
            StartCoroutine(LevelLoader.LoadLevel("Menu"));
        }
    }

}
