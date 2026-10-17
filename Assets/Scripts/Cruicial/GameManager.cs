using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{   public ObjectPoolManager objectPool;

    //Other Stuff
    static GameManager instance;
    [SerializeField] GameObject gameOverSelectedObject;
    [SerializeField] GameObject gamePauseSelectedObject;
    [SerializeField] GameObject gameOverMenu;
    [SerializeField] GameObject gamePauseMenu;

    public PlayerShip playerObject;
    public GameObject[] playerUIToHideOnPause;


    public enum GameState
    {
        GAME_PLAY,
        GAME_PAUSED,
        GAME_OVER,
    }
    public GameState state;
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
        if(!playerObject) playerObject = GameObject.Find("PlayerShip").GetComponent<PlayerShip>();

        instance = this;
        eventSystem = GetComponent<EventSystem>();

    }

    void Start()
    {
        InputManager.ui.Pause.performed += Pause_performed;
        InputManager.ui.UnPause.performed += UnPause_performed;
        InputManager.ui.ChangeUISelectorToGamepad.performed += ChangeUISelectorGamepad_performed;
        InputManager.ui.ChangeUISelectorToMouse.performed += ChangeUISelectorToMouse_performed;
    }

    void OnDestroy()
    {
        InputManager.ui.Pause.performed -= Pause_performed;
        InputManager.ui.UnPause.performed -= UnPause_performed;
        InputManager.ui.ChangeUISelectorToGamepad.performed -= ChangeUISelectorGamepad_performed;
        InputManager.ui.ChangeUISelectorToMouse.performed -= ChangeUISelectorToMouse_performed;
    }

    void Update()
    {
        if (state == GameState.GAME_OVER)
        {
            if (gameOverTimer > 0)
            {
                for (int i = 0; i < playerUIToHideOnPause.Length; i++)
                {
                    playerUIToHideOnPause[i].SetActive(false);
                }
                if (playerObject.gameObject.activeSelf)
                {
                    playerObject.camera.transform.parent = transform;
                    playerObject.gameObject.SetActive(false);
                }
                gameOverTimer -= Time.deltaTime;
            }
            else
            {
                OpenGameOverMenu();
            }
        }
    }

    private void ChangeUISelectorToMouse_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(state != GameState.GAME_PLAY)
        {
            if (eventSystem.currentSelectedGameObject) eventSystem.SetSelectedGameObject(null);
        }
    }

    private void ChangeUISelectorGamepad_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (state != GameState.GAME_PLAY)
        {
            if(eventSystem.currentSelectedGameObject == null)
            {
                if (gamePauseMenu.activeSelf)
                {
                    eventSystem.SetSelectedGameObject(gamePauseSelectedObject);
                }
                else if (gameOverMenu.activeSelf)
                {
                    eventSystem.SetSelectedGameObject(gameOverSelectedObject);
                }
            }

        }
    }

    private void UnPause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(state == GameState.GAME_PAUSED)
        {
            Resume();
        }
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(state == GameState.GAME_PAUSED)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    
    public void OpenGameOverMenu()
    {
        if (!gameOverActive)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            gameOverMenu.SetActive(true);
            if (InputManager.controlScheme == InputManager.ControlScheme.GAMEPAD) eventSystem.SetSelectedGameObject(gameOverSelectedObject);
            else eventSystem.SetSelectedGameObject(null);
            gameOverActive = true;
        }
    }

    public void Pause()
    {
        Time.timeScale = 0;
        Cursor.visible = true;
        foreach (GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(false);
        }
        if (InputManager.controlScheme == InputManager.ControlScheme.GAMEPAD) eventSystem.SetSelectedGameObject(gamePauseSelectedObject);
        else eventSystem.SetSelectedGameObject(null);
        state = GameState.GAME_PAUSED;
        gamePauseMenu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        Cursor.visible = false;
        foreach (GameObject playerUI in playerUIToHideOnPause)
        {
            playerUI.SetActive(true);
        }
        state = GameState.GAME_PLAY;
        gamePauseMenu.SetActive(false);
    }

    public void Restart()
    {
        Cursor.visible = false;
        StartCoroutine(SceneLoader.instance.Load(SceneManager.GetActiveScene().buildIndex));
        Time.timeScale = 1;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        Cursor.visible = true;
        StartCoroutine(SceneLoader.instance.Load("Title"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
