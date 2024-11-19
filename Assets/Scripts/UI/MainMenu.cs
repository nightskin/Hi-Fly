using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void OpenSettings()
    {
        StartCoroutine(SceneLoader.instance.LoadLevel("Settings"));
    }
    
    public void PlayGame()
    {
        StartCoroutine(SceneLoader.instance.LoadLevel("Game"));
    }

    public void NewGame()
    {
        StartCoroutine(SceneLoader.instance.LoadLevel("Intro"));
    }

    public void Continue()
    {
        StartCoroutine(SceneLoader.instance.LoadLevel("Hub"));
    }

    public void BackToMenu()
    {
        StartCoroutine(SceneLoader.instance.LoadLevel("Menu"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
