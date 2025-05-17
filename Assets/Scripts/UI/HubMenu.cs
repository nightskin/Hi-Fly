using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HubMenu : MonoBehaviour
{
    public static string levelName = string.Empty;
    public static bool open = false;
    [SerializeField] Text label;

    public void OpenLevelMenu()
    {
        open = true;
        gameObject.SetActive(true);
        label.text = "Enter " + levelName + "?";
        GameManager.eventSystem.SetSelectedGameObject(gameObject.transform.GetChild(0).GetChild(0).GetChild(0).gameObject);
        Time.timeScale = 0;
    }

    public void Yes()
    {
        Time.timeScale = 1;
        open = false;
        SceneManager.LoadScene(levelName);
        //StartCoroutine(SceneLoader.instance.LoadLevel(levelName));
    }

    public void No()
    {
        Time.timeScale = 1;
        open = false;
        gameObject.SetActive(false);
    }
    
}
