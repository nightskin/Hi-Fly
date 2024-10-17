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

    public void NewGame()
    {
        StartCoroutine(LevelLoader.instance.LoadLevel("Intro"));
    }

    public void Continue()
    {
        StartCoroutine(LevelLoader.instance.LoadLevel("Hub"));
    }

    public void BackToMenu()
    {
        StartCoroutine(LevelLoader.instance.LoadLevel("Menu"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
