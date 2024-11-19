using UnityEngine;

public class HubWorld : MonoBehaviour
{
    public static string levelId = string.Empty;
    public static GameObject planetMenu;
    void Start()
    {
        planetMenu = transform.Find("PlanetMenu").gameObject;
    }

    public static void OpenPlanetMenu()
    {
        GameManager.eventSystem.SetSelectedGameObject(planetMenu.transform.GetChild(0).GetChild(1).gameObject);
        Time.timeScale = 0;
        planetMenu.SetActive(true);
    }

    public void Yes()
    {
        Time.timeScale = 1;
        StartCoroutine(SceneLoader.instance.LoadLevel("Level"));
        planetMenu.SetActive(false);
    }

    public void No()
    {
        Time.timeScale = 1;
        planetMenu.SetActive(false);
    }
    
}
