using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;

    public void OpenSettings()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Settings"));
    }
    
    public void PlayGame()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("LevelSelect"));
    }

    public void SelectLevel1()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("1"));
    }

    public void BackToMenu()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Menu"));
    }

    public void QuitGame()
    {
        eventSystem.gameObject.SetActive(false);
        Application.Quit();
    }

}
