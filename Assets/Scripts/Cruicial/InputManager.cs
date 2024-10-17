using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerShipInput shipInput;

    void Awake()
    {
        shipInput = new PlayerShipInput();
        shipInput.Enable();
    }

    void OnDisable()
    {
        shipInput.Disable();
    }

}
