using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OpenSettings()
    {
        StartCoroutine(LevelLoader.LoadLevel("Settings"));
    }


    public void PlayGame()
    {
        StartCoroutine(LevelLoader.LoadLevel("Game"));
    }


    public void QuitGame()
    {
        Application.Quit();
    }

}
