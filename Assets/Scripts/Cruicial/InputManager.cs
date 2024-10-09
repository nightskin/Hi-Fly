using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;
    public static PlayerShipInput shipInput;

    void Awake()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();

        shipInput = new PlayerShipInput();
        shipInput.Enable();
    }
}
