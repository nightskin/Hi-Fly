using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    [SerializeField] List<InventoryItem> starterInventory = new List<InventoryItem>();
    [SerializeField] Image inventorySelectImage;
    [SerializeField] Text inventoryCountText;
    public static List<InventoryItem> inventory = new List<InventoryItem>();
    public static int inventoryIndex = 0;

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

    public static List<Vector3> onRailsPath;

    [SerializeField] Text scoreText;
    public static int score = 0;
    
    void Awake()
    {
        //For Debug Purposes
        inventory = starterInventory;

        //Needed For OnRailsMovement
        GameObject path = GameObject.Find("Path");
        if (path)
        {
            //onRailsPath = path.GetComponent<>();
        }
    }

    void Start()
    {
        UpdateInventoryUI();

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
        InputManager.input.Player.ToggleInventory.performed += ToggleInventory_performed;
    }

    private void ToggleInventory_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(inventory.Count > 0) 
        {
            if (obj.ReadValue<float>() > 0)
            {
                if (inventoryIndex < inventory.Count - 1)
                {
                    inventoryIndex++;
                }
                else
                {
                    inventoryIndex = 0;
                }
            }
            else if (obj.ReadValue<float>() < 0)
            {
                if (inventoryIndex > 0)
                {
                    inventoryIndex--;
                }
                else
                {
                    inventoryIndex = inventory.Count - 1;
                }
            }
            UpdateInventoryUI();
        }
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
        InputManager.input.Player.ToggleInventory.performed -= ToggleInventory_performed;
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

    public void UpdateInventoryUI()
    {
        inventorySelectImage.sprite = inventory[inventoryIndex].image;
        inventoryCountText.text = inventory[inventoryIndex].stock.ToString();
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
