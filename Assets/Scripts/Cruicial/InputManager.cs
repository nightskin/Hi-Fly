using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public enum ControlScheme
    {
        DESKTOP,
        GAMEPAD,
    }

    static InputActions input;
    public static InputActions.PlayerActions player;
    public static InputActions.UIActions ui;
    public static ControlScheme controlScheme;

    void Awake()
    {
        input = new InputActions();
        input.Enable();
        player = input.Player;
        ui = input.UI;

        ui.ChangeControlsGamepad.performed += ChangeControlsGamepad_performed;
        ui.ChangeControlsMouseAndKeyboard.performed += ChangeControlsDesktop_performed;

    }

    private void ChangeControlsDesktop_performed(InputAction.CallbackContext obj)
    {
        controlScheme = ControlScheme.DESKTOP;
    }

    private void ChangeControlsGamepad_performed(InputAction.CallbackContext obj)
    {
        controlScheme = ControlScheme.GAMEPAD;
    }

    void OnDisable()
    {
        player.Disable();
        ui.Disable();
        input.Disable();
    }

    void OnDestroy()
    {
        ui.ChangeControlsGamepad.performed -= ChangeControlsGamepad_performed;
        ui.ChangeControlsMouseAndKeyboard.performed -= ChangeControlsDesktop_performed;
    }

}
