using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButtons : MonoBehaviour
{
    [SerializeField] EventSystem eventSystem;
    int selectedLevel = 0;

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

    public void ChangeSelectedLevel()
    {

    }

    public void SelectLevel()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load(selectedLevel.ToString()));
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
