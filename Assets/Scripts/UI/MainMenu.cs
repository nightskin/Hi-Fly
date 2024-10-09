using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OpenSettings()
    {
        StartCoroutine(LevelLoader.instance.LoadLevel("Settings"));
    }


    public void PlayGame()
    {
        StartCoroutine(LevelLoader.instance.LoadLevel("Game"));
    }


    public void QuitGame()
    {
        Application.Quit();
    }

}
