using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static PlayerInput input;

    void Awake()
    {
        input = new PlayerInput();
        input.Enable();
    }
}
