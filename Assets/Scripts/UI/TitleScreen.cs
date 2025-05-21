using UnityEngine;
using UnityEngine.InputSystem;

public class TitleScreen : MonoBehaviour
{
    void Start()
    {
        InputManager.input.Player.StartGame.performed += StartGame_performed;    
    }

    void StartGame_performed(InputAction.CallbackContext obj)
    {
        StartCoroutine(SceneLoader.instance.Load("Menu"));
    }

    void OnDestroy()
    {
        InputManager.input.Player.StartGame.performed -= StartGame_performed;
    }
}
