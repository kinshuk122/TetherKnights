using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    protected EnemyStateMachine model;
    
    public BaseState(EnemyStateMachine StateMachine)
    {
        model = StateMachine;
    }
    
    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

}
