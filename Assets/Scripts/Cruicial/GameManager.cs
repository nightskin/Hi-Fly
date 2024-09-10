using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;

    public static Player player;
    public GameObject[] playerUI;

    public static string seed = "Dota2 < League Of Legends";
    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public static bool gameOver = false;
    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;
    public static AudioSource bgm;

    float gameOverTimer = 1;
    bool gameOverActive = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        eventSystem = GetComponent<EventSystem>();
        sceneNodeManager = GetComponent<SceneNodeManager>();
        bgm = GetComponent<AudioSource>();

        InputManager.input.actions.Pause.performed += Pause_performed;
        InputManager.input.actions.UnPause.performed += UnPause_performed;
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
                    for(int i = 0; i < playerUI.Length; i++) 
                    {
                        playerUI[i].SetActive(false);
                    }
                    gameOverMenu.SetActive(true);
                    eventSystem.SetSelectedGameObject(gameOverMenu.transform.GetChild(1).gameObject);
                    gameOverActive = true;
                }   
            }
        }
    }

    void OnDisable()
    {
        InputManager.input.actions.Pause.performed -= Pause_performed;
        InputManager.input.actions.UnPause.performed -= UnPause_performed;
    }

    public void Pause()
    {
        Time.timeScale = 0;
        gamePauseMenu.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        gamePauseMenu.SetActive(false);
    }

    public void Restart()
    {
        gameOver = false;
        StartCoroutine(LevelLoader.LoadLevel("Game"));
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        gameOver = false;
        StartCoroutine(LevelLoader.LoadLevel("Title"));
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
