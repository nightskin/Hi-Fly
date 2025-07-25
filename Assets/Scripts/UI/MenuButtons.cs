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
        StartCoroutine(SceneLoader.instance.Load("CharacterSelect"));
    }

    public void SelectJetFly()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Game"));
    }
    
    public void SelectHoverShot()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Game"));
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
