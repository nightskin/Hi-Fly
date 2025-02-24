using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;

    public void OpenSettings()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.LoadLevel("Settings"));
    }
    
    public void PlayGame()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.LoadLevel("PlayGame"));
    }

    public void NewGame()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.LoadLevel("Intro"));
    }

    public void Continue()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.LoadLevel("Hub"));
    }

    public void BackToMenu()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.LoadLevel("Menu"));
    }

    public void QuitGame()
    {
        eventSystem.gameObject.SetActive(false);
        Application.Quit();
    }

}
