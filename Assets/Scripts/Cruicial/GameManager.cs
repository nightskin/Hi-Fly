using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        TUTORIAL,
        SURVIVOR,
    }
    public GameMode gameMode;

    public enum PlayerMovement
    {
        ALL_RANGE,
        ON_RAILS,
    }
    public PlayerMovement playerMovement;


    public ObjectPoolManager objectPool;

    //Other Stuff
    static GameManager instance;
    [SerializeField] GameObject gameOverSelectedObject;
    [SerializeField] GameObject upgradeMenu;
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject gamePauseMenu;

    [HideInInspector] public PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;

    [HideInInspector] public bool gameOver;
    [HideInInspector] public bool gamePaused;
    [HideInInspector] public EventSystem eventSystem;

    float gameOverTimer = 1;
    bool gameOverActive = false;
    


    public static GameManager Get()
    {
        return instance;
    }

    void Awake()
    {
        if (!objectPool) objectPool = GameObject.Find("ObjectPool").GetComponent<ObjectPoolManager>();

        instance = this;

        gameOver = false;
        gamePaused = false;

        playerShip = transform.Find("PlayerShip").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
        
    }

    void Start()
    {
        CloseUpgradeMenu();
        InputManager.input.UI.Pause.performed += Pause_performed;
        InputManager.input.UI.UnPause.performed += UnPause_performed;
        InputManager.input.UI.ChangeUISelectorToGamepad.performed += ChangeUISelectorGamepad_performed;
        InputManager.input.UI.ChangeUISelectorToMouse.performed += ChangeUISelectorToMouse_performed;
    }

    private void ChangeUISelectorToMouse_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(gamePaused || gameOver)
        {
            eventSystem.firstSelectedGameObject = null;
        }
    }

    private void ChangeUISelectorGamepad_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (gamePaused || gameOver)
        {
            if(gamePauseMenu.activeSelf)
            {
                eventSystem.SetSelectedGameObject(gamePauseMenu.transform.GetChild(0).transform.GetChild(1).gameObject);
            }
            else if(gameOverMenu.activeSelf)
            {
                eventSystem.SetSelectedGameObject(gameOverSelectedObject);
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
        InputManager.input.UI.Pause.performed -= Pause_performed;
        InputManager.input.UI.UnPause.performed -= UnPause_performed;
        InputManager.input.UI.ChangeUISelectorToGamepad.performed -= ChangeUISelectorGamepad_performed;
        InputManager.input.UI.ChangeUISelectorToMouse.performed -= ChangeUISelectorToMouse_performed;
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

    public void OpenUpgradeMenu()
    {
        if (upgradeMenu)
        {
            Time.timeScale = 0;
            Cursor.visible = true;
            gamePaused = true;
            foreach (GameObject playerUI in playerUIToHideOnPause)
            {
                playerUI.SetActive(false);
            }
            upgradeMenu.SetActive(true);
        }
    }

    public void CloseUpgradeMenu()
    {
        if (upgradeMenu)
        {
            Time.timeScale = 1;
            Cursor.visible = false;
            gamePaused = false;
            foreach (GameObject playerUI in playerUIToHideOnPause)
            {
                playerUI.SetActive(true);
            }
            upgradeMenu.SetActive(false);

            if(EnemyWaveManager.Get())
            {
                EnemyWaveManager.Get().UpdateUI();
            }

        }
    }
}
