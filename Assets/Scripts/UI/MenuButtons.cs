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
    
    public void PlayerSelect()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("PlayerSelect"));
    }

    public void GameModeSelect()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("GameMode"));
    }

    public void OpenTutorial()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Tutorial"));
    }

    public void RogueMode()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Rogue"));
    }

    public void SurvivorMode()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Survivor"));
    }

    public void AssassinMode()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Assassin"));
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
