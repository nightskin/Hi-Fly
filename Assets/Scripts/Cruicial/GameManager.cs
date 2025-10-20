using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        TUTORIAL,
        ROGUE,
        SURVIVOR,
        ASSASSIN,
    }
    public GameMode gameMode;

    public enum PlayerMode
    {
        ALL_RANGE,
        ON_RAILS,
    }
    public PlayerMode playerMode;

    public enum PlayerWeapon
    {
        CHARGE_BULLET,
        POWER_BEAM,
        RAPID_BULLET,
    }
    public PlayerWeapon playerWeapon;



    //Other Stuff
    static GameManager instance;
    [SerializeField] GameObject gameOverSelectedObject;
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject gamePauseMenu;

    [HideInInspector] public PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;

    [HideInInspector] public bool gameOver;
    [HideInInspector] public bool gamePaused;
    [HideInInspector] public bool levelBeaten;
    [HideInInspector] public EventSystem eventSystem;

    float gameOverTimer = 1;
    bool gameOverActive = false;
    


    public static GameManager Get()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;

        gameOver = false;
        levelBeaten = false;
        gamePaused = false;

        playerShip = transform.Find("PlayerShip").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
    }

    void Start()
    {
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
        foreach (GameObject playerUI in playerUIToHideOnPause)
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
