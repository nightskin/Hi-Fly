using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject gameOverSelectedObject;
    public enum Difficulty
    {
        EASY,
        NORMAL,
        HARD,
    }
    public static Difficulty difficulty = Difficulty.NORMAL;

    public enum PlayerMode
    {
        ALL_RANGE,
        ON_RAILS,
        TPS,
    }
    public static PlayerMode playerMode;
    [SerializeField] PlayerMode startPlayerMode;

    public enum PlayerPowerUp
    {
        NONE,
        POWER_BOMB,
        POWER_BEAM,
    }
    public static PlayerPowerUp currentPowerUp;
    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public static float aimSensitivy = 1000;

    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;


    public static PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;

    public static bool gameOver = false;
    public static bool gamePaused = false;
    public static bool gameBeaten = false;
    

    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;

    float gameOverTimer = 1;
    bool gameOverActive = false;
    

    void Start()
    {
        playerMode = startPlayerMode;
        currentPowerUp = PlayerPowerUp.NONE;
        playerShip = transform.Find("PlayerShip").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
        sceneNodeManager = GetComponent<SceneNodeManager>();

        InputManager.input.Player.Pause.performed += Pause_performed;
        InputManager.input.Player.UnPause.performed += UnPause_performed;
    }


    private void UnPause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(gamePaused)
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
                for (int i = 0; i < playerUIToHideOnPause.Length; i++)
                {
                    playerUIToHideOnPause[i].SetActive(false);
                }
                playerShip.gameObject.SetActive(false);
                gameOverTimer -= Time.deltaTime;
            }
            else
            {
                if (!gameOverActive)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    gameOverMenu.SetActive(true);
                    eventSystem.SetSelectedGameObject(gameOverSelectedObject);
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
    
    public void Pause()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        foreach(GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(false);
        }
        eventSystem.SetSelectedGameObject(gamePauseMenu.transform.GetChild(0).transform.GetChild(1).gameObject);
        gamePaused = true;
        gamePauseMenu.SetActive(gamePaused);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
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
        StartCoroutine(SceneLoader.instance.Load(SceneManager.GetActiveScene().buildIndex));
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        Cursor.visible = true;
        gameOver = false;
        gamePaused = false;
        StartCoroutine(SceneLoader.instance.Load("Title"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
