using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public enum PlayerMode
    {
        STANDARD_MODE,
        ON_RAILS_MODE,
        STRAFE_MODE,
    }
    public static PlayerMode playerMode;
    
    [SerializeField] PlayerMode startPlayerMode;

    public enum Difficulty
    {
        EASY,
        NORMAL,
        HARD
    }
    public static Difficulty difficulty = Difficulty.NORMAL;

    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public static float aimSensitivy = 1000;

    public static bool invertLookY = false;
    public static bool invertSteerY = true;

    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;

    public GameObject[] miniMap;

    public static PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;
    public static float isoLevel = 0;
    public static bool gameOver = false;
    public static bool gamePaused = false;
    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;

    float gameOverTimer = 1;
    bool gameOverActive = false;

    void Start()
    {
        playerMode = startPlayerMode;
        playerShip = transform.Find("PlayerShip").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
        sceneNodeManager = GetComponent<SceneNodeManager>();
        
        if(SceneManager.GetActiveScene().name == "Hub")
        {
            foreach(GameObject obj in miniMap)
            {
                obj.SetActive(true);
            }
        }

        InputManager.input.Player.Pause.performed += Pause_performed;
        InputManager.input.Player.UnPause.performed += UnPause_performed;
        InputManager.input.Player.AutoPilot.performed += AutoPilot_performed;
    }

    private void AutoPilot_performed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        miniMap[0].GetComponent<RectTransform>().sizeDelta = new Vector2(700, 309);

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
        InputManager.input.Player.AutoPilot.performed -= AutoPilot_performed;
    }

    public void Pause()
    {
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
