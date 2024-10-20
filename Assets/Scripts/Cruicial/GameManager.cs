using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;

    public static PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;

    public static string seed = "Dota2 < League Of Legends";
    public static bool gameOver = false;
    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;

    float gameOverTimer = 1;
    bool gameOverActive = false;

    void Start()
    {
        playerShip = GameObject.FindWithTag("Player").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
        sceneNodeManager = GetComponent<SceneNodeManager>();
        

        InputManager.input.Player.Pause.performed += Pause_performed;
        InputManager.input.Player.UnPause.performed += UnPause_performed;
    }

    private void UnPause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(gamePauseMenu.activeSelf)
        {
            Resume();
        }
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(!gameOver)
        {
            if (gamePauseMenu.activeSelf)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void Update()
    {
        if(gameOver)
        {
            if (gameOverTimer > 0)
            {
                gameOverTimer -= Time.deltaTime;
            }
            else
            {
                if (!gameOverActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    for(int i = 0; i < playerUIToHideOnPause.Length; i++) 
                    {
                        playerUIToHideOnPause[i].SetActive(false);
                    }
                    playerShip.GetComponent<MeshRenderer>().enabled = false;
                    gameOverMenu.SetActive(true);
                    eventSystem.SetSelectedGameObject(gameOverMenu.transform.GetChild(1).gameObject);
                    gameOverActive = true;
                }   
            }
        }
    }

    void OnDisable()
    {
        InputManager.input.Player.Pause.performed -= Pause_performed;
        InputManager.input.Player.UnPause.performed -= UnPause_performed;
    }

    public void Pause()
    {
        Time.timeScale = 0;
        foreach(GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(false);
        }
        gamePauseMenu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        foreach (GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(true);
        }
        gamePauseMenu.SetActive(false);
    }

    public void Restart()
    {
        gameOver = false;
        StartCoroutine(LevelLoader.instance.LoadLevel(SceneManager.GetActiveScene().buildIndex));
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        gameOver = false;
        StartCoroutine(LevelLoader.instance.LoadLevel("Title"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
