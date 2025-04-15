using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public enum Difficulty
    {
        EASY,
        NORMAL,
        HARD
    }
    public static Difficulty difficulty = Difficulty.NORMAL;

    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;

    public static PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;

    public static string seed = "Dota2 < LOL";
    public static Noise noise = new Noise(seed.GetHashCode());
    public static float isoLevel = 0;
    public static bool gameOver = false;
    public static bool gamePaused = false;
    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;

    float gameOverTimer = 1;
    bool gameOverActive = false;

    void Start()
    {
        playerShip = transform.Find("Mesh").GetComponent<PlayerShip>();
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
            if (gamePaused)
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
                    playerShip.gameObject.SetActive(false);
                    gameOverMenu.SetActive(true);
                    eventSystem.SetSelectedGameObject(gameOverMenu.transform.GetChild(1).gameObject);
                    gameOverActive = true;
                }   
            }
        }
    }

    void OnDestroy()
    {
        InputManager.input.Player.Pause.performed -= Pause_performed;
        InputManager.input.Player.UnPause.performed -= UnPause_performed;
    }

    public static void InitRandom()
    {
        Random.InitState(seed.GetHashCode());
        noise = new Noise(seed.GetHashCode());
    }

    public void Pause()
    {
        Cursor.visible = true;
        Time.timeScale = 0;
        foreach(GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(false);
        }
        eventSystem.SetSelectedGameObject(gamePauseMenu.transform.GetChild(1).gameObject);
        gamePaused = true;
        gamePauseMenu.SetActive(gamePaused);
    }

    public void Resume()
    {
        Cursor.visible = false;
        Time.timeScale = 1;
        foreach (GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(true);
        }
        gamePaused = false;
        gamePauseMenu.SetActive(gamePaused);
    }

    public void Restart()
    {
        Cursor.visible = false;
        gameOver = false;
        gamePaused = false;
        StartCoroutine(SceneLoader.instance.LoadLevel(SceneManager.GetActiveScene().buildIndex));
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Cursor.visible = true;
        Time.timeScale = 1;
        gameOver = false;
        gamePaused = false;
        StartCoroutine(SceneLoader.instance.LoadLevel("Title"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
