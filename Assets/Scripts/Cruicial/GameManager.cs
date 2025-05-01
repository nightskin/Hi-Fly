using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Splines;

public class GameManager : MonoBehaviour
{
    public enum Difficulty
    {
        EASY,
        NORMAL,
        HARD,
    }
    public static Difficulty difficulty = Difficulty.NORMAL;

    public enum PlayerMode
    {
        STANDARD_MODE,
        STRAFE_MODE,
        ON_RAILS_MODE
    }
    public static PlayerMode playerMode;
    [SerializeField] PlayerMode startPlayerMode;

    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public static float aimSensitivy = 1000;

    public static bool invertLookY = false;
    public static bool invertSteerY = true;

    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;
    public GameObject miniMap;
    public Camera miniMapCamera;

    public static PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;
    public static float isoLevel = 0;

    public static bool gameOver = false;
    public static bool gamePaused = false;
    public static bool gameBeaten = false;
    

    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;

    float gameOverTimer = 1;
    bool gameOverActive = false;

    public static SplineContainer onRailsPath;
    public static float onRailsPathLength;
    
    void Start()
    {
        onRailsPath = GameObject.Find("Path0").GetComponent<SplineContainer>();
        if(onRailsPath) onRailsPathLength = onRailsPath.CalculateLength();

        playerMode = startPlayerMode;
        playerShip = transform.Find("PlayerShip").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
        sceneNodeManager = GetComponent<SceneNodeManager>();

        if(SceneManager.GetActiveScene().name == "Hub")
        {
            miniMapCamera.gameObject.SetActive(true);
            miniMap.SetActive(true);
        }
        else
        {
            miniMapCamera.gameObject.SetActive(false);
            miniMap.SetActive(false);
        }

        InputManager.input.Player.Pause.performed += Pause_performed;
        InputManager.input.Player.UnPause.performed += UnPause_performed;
        InputManager.input.Player.ToggleMiniMap.performed += ToggleMiniMap_performed;
    }

    private void ToggleMiniMap_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(SceneManager.GetActiveScene().name == "Hub")
        {
            if(!miniMap.activeSelf)
            {
                miniMapCamera.gameObject.SetActive(true);
                miniMap.SetActive(true);
            }
            else
            {
                miniMapCamera.gameObject.SetActive(false);
                miniMap.SetActive(false);
            }
        }
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
                    eventSystem.SetSelectedGameObject(gameOverMenu.transform.GetChild(0).transform.GetChild(1).gameObject);
                    gameOverActive = true;
                }   
            }
        }
    }

    void OnDestroy()
    {
        InputManager.input.Player.Pause.performed -= Pause_performed;
        InputManager.input.Player.UnPause.performed -= UnPause_performed;
        InputManager.input.Player.ToggleMiniMap.performed -= ToggleMiniMap_performed;
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
        StartCoroutine(SceneLoader.instance.LoadLevel(SceneManager.GetActiveScene().buildIndex));
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        Cursor.visible = true;
        gameOver = false;
        gamePaused = false;
        StartCoroutine(SceneLoader.instance.LoadLevel("Title"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
