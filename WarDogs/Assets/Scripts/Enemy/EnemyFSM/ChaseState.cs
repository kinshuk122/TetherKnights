using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : BaseState
{
    private NavMeshAgent agent;
    private Transform target;
    private PlayerStats playerStats;
    private List<Transform> targets;
    private GameObject firePoint;
    private float attackRange;
    
    
    [Header("Bools")]
    public bool playerInLineOfSight;
    private bool playerInAttackRange;
    private bool isActive;
    
    public ChaseState(EnemyStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        model.stateName = "Chase";

        playerInLineOfSight = false;
        playerInAttackRange = false;
        agent = model.agent;
        target = model.target;
        playerStats = model.playerStats;
        targets = model._targets;
        firePoint = model.firePoint;
        attackRange = model.attackRange.Value;
        
        ChasePlayer();
    }

    public override void Update()
    {
        base.Update();
        
        Vector3 raycastOrigin = firePoint != null ? firePoint.transform.position : agent.transform.position + Vector3.up;

        Collider[] collidersInAttackRange = Physics.OverlapSphere(agent.transform.position, attackRange);

        foreach (var collider in collidersInAttackRange)
        {
            if (collider.transform == target)
            {
                playerInAttackRange = true;

                Vector3 directionToPlayer = (target.position - raycastOrigin).normalized;
                RaycastHit hit;
                int layerMask = LayerMask.GetMask("Default", "Player");
                Debug.DrawRay(raycastOrigin, directionToPlayer * attackRange, Color.red);

                if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, attackRange, layerMask))
                {
                    if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("PermanentPart"))
                    {
                        playerInLineOfSight = true;
                    }
                }

                break;
            }
        }

        if (playerInLineOfSight && playerInAttackRange)
        {
            model.ChangeState(model.attackState);
            model.playerInLineOfSight = true;
        }
        else
        {
            agent.SetDestination(target.position); 
        }
        
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private void ChasePlayer()
    {
        if (!agent.enabled)
        {
            Debug.LogWarning("NavMeshAgent was disabled. Enabling now.");
            agent.enabled = true;
        }

        // Check if agent is on the NavMesh
        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(agent.transform.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position); 
            }
            else
            {
                return;
            }
        }
        
        if (playerStats != null && playerStats.isDead.Value)
        {
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                PlayerStats otherPlayerStats = targets[i].GetComponentInChildren<PlayerStats>();
                if (otherPlayerStats != null && !otherPlayerStats.isDead.Value)
                {
                    target = targets[i];
                    playerStats = otherPlayerStats;
                    break;
                }
            }
        }

        if (!target.gameObject.activeInHierarchy)
        {
            isActive = false;
            while (!isActive)
            {
                target = targets[Random.Range(0, targets.Count)];
                if (target.gameObject.activeInHierarchy)
                {
                    isActive = true;
                }
            }
        }

        if (target != null)
        {
            agent.SetDestination(target.position);
            isActive = false;
        }
    }

}
