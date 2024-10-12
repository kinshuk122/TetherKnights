using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DodgeState : BaseState
{
    private float dodgeSpeed;
    private bool isDodging = false;
    private float dodgeCooldown;
    private float dodgeTimer = 0f;
    private Vector3 dodgePosition;
    private NavMeshAgent agent;
    private Transform target;
    
    public DodgeState(EnemyStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        model.stateName = "Dodge";

        agent = model.agent;
        dodgeSpeed = model.dodgeSpeed;
        target = model.target;
        dodgeTimer = model.dodgeTimer;
        int dodgeDirection = (Random.Range(0, 3) * 2 - 1) * 3; // Generates either -3 or 3
        Vector3 towardsPlayer = (target.position - agent.transform.position).normalized;
        Vector3 dodgeVector = Vector3.Cross(towardsPlayer, Vector3.up) * dodgeDirection;
        dodgePosition = agent.transform.position + dodgeVector;

        isDodging = true;
    }

    public override void Update()
    {
        base.Update();
        
        if (isDodging)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, dodgePosition, dodgeSpeed * Time.deltaTime);

            if (Vector3.Distance(agent.transform.position, dodgePosition) < 1.5f)
            {
                isDodging = false;
                model.dodgeTimer = 0f;
                model.ChangeState(model.attackState); //what if model is stuck at dodge state/cannot reach the dodge position
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
    

}

