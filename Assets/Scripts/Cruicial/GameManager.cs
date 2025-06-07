using UnityEngine;
using UnityEngine.UI;
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

    //For ItemManagment
    public enum PowerUps
    {
        NONE,
        MISSILE,
        LAZER,
        RAPID_FIRE,
    }
    public static PowerUps currentPowerUp = PowerUps.NONE;
    [SerializeField] Image powerUpImage;

    public enum PlayerMode
    {
        All_RANGE_MODE,
        ON_RAILS_MODE,
    }
    public static PlayerMode playerMode;
    [SerializeField] PlayerMode startPlayerMode;

    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public static float aimSensitivy = 1000;

    public GameObject gameOverMenu;
    public GameObject gamePauseMenu;
    public GameObject miniMap;
    public Camera miniMapCamera;


    public static PlayerShip playerShip;
    public GameObject[] playerUIToHideOnPause;

    public static bool gameOver = false;
    public static bool gamePaused = false;
    public static bool gameBeaten = false;
    

    public static EventSystem eventSystem;
    public static SceneNodeManager sceneNodeManager;

    float gameOverTimer = 1;
    bool gameOverActive = false;

    public static SplineContainer splinePath;
    public static float splinePathLength = 0;

    [SerializeField] Text scoreText;
    public static int score = 0;
    

    void Start()
    {
        //Needed For OnRailsMovement
        GameObject path = GameObject.Find("Path");
        if (path)
        {
            splinePath = path.GetComponent<SplineContainer>();
            splinePathLength = splinePath.CalculateLength();
        }
        else
        {
            splinePathLength = 0;
            playerMode = PlayerMode.All_RANGE_MODE;
        }

        currentPowerUp = PowerUps.LAZER;
        UpdatePowerUpUI(null);

        scoreText.text = score.ToString();
        playerMode = startPlayerMode;
        playerShip = transform.Find("PlayerShip").GetComponent<PlayerShip>();
        eventSystem = GetComponent<EventSystem>();
        sceneNodeManager = GetComponent<SceneNodeManager>();

        if(SceneManager.GetActiveScene().name != "Hub")
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
    
    public void UpdatePowerUpUI(Sprite sprite)
    {
        if(sprite == null)
        {
            powerUpImage.color = Color.clear;
        }
        else
        {
            powerUpImage.color = Color.white;
        }
        powerUpImage.sprite = sprite;
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();
    }

    public void SubractScore(int amount) 
    {
        score -= amount;
        scoreText.text = score.ToString();
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
