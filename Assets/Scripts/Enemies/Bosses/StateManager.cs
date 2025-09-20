using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class StateManager<Estate> : MonoBehaviour where Estate : Enum
{
    protected Dictionary<Estate, BaseState<Estate>> States = new Dictionary<Estate, BaseState<Estate>>();
    protected BaseState<Estate> currentState;
    private bool isSwitchingState = false;

    public void SwitchState(Estate nextState)
    {
        isSwitchingState = true;
        currentState.ExitState();
        currentState = States[nextState];
        currentState.EnterState();
        isSwitchingState = false;
    }

    void Start()
    {
        currentState.EnterState();
    }

    void Update()
    {
        Estate nextStateKey = currentState.NextState();

        if (!isSwitchingState && nextStateKey.Equals(currentState.stateKey))
        {
            currentState.UpdateState();
        }
        else if(!isSwitchingState)
        {
            SwitchState(nextStateKey);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        currentState.OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other)
    {
        currentState.OnTriggerStay(other);
    }

    void OnTriggerExit(Collider other)
    {
        currentState.OnTriggerExit(other);
    }
}
