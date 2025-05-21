using UnityEngine;
using UnityEngine.UI;

public class HubMenu : MonoBehaviour
{
    public static string levelName;
    public static bool open;
    [SerializeField] Text label;
    [SerializeField] GameObject selectionStart;

    void Start()
    {
        open = false;    
    }

    public void OpenLevelMenu()
    {
        open = true;
        gameObject.SetActive(true);
        label.text = "Enter " + levelName + "?";
        GameManager.eventSystem.SetSelectedGameObject(selectionStart);
        Time.timeScale = 0;
    }

    public void Yes()
    {
        Time.timeScale = 1;
        open = false;
        GameManager.gamePaused = true;
        StartCoroutine(SceneLoader.instance.Load("LevelTransitionScene"));
    }

    public void No()
    {
        Time.timeScale = 1;
        open = false;
        gameObject.SetActive(false);
    }
    
}
