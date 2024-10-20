using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputActions input;

    void Awake()
    {
        input = new InputActions();
        input.Enable();
    }

    void OnDisable()
    {
        input.Disable();
    }

}
