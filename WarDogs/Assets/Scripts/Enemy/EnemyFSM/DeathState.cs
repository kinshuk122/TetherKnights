using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DeathState : BaseState
{
    private NavMeshAgent agent;
    
    public DeathState(EnemyStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        model.stateName = "Death";

        agent = model.agent;
        
        WaveSpawner.instance.enemiesAlive--;
        agent.gameObject.SetActive(false);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
