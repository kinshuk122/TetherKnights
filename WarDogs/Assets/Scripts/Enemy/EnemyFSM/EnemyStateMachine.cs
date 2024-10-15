using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyStateMachine : NetworkBehaviour
{
    public BaseState _currentState;
    public string stateName;
    
    [Header("State Checking Booleans")]
    public bool playerInLineOfSight;
    
    [Header("General")]
    public EnemyAIScriptableObject enemyType;
    public NavMeshAgent agent;
    public float baseOffset; //Change it to 4 once camera works perfectly
    public Transform target;
    public PlayerStats playerStats;
    public List<Transform> _targets = new List<Transform>();
    public GameObject firePoint;
    public EnemyAIScriptableObject[] enemyAiScriptable;
    
    [Header("States")]
    public ChaseState chaseState;
    public AttackState attackState;
    public DodgeState dodgeState;
    public DeathState deathState;

    [Header("ScriptableObject References")] 
    public NetworkVariable<float> health = new NetworkVariable<float>();
    public NetworkVariable<float> damage = new NetworkVariable<float>();
    public NetworkVariable<float> sightRange = new NetworkVariable<float>();
    public NetworkVariable<float> attackRange = new NetworkVariable<float>();
    public NetworkVariable<float> speed = new NetworkVariable<float>();
    public NetworkVariable<float> increaseSpeedOnGettingAttacked = new NetworkVariable<float>();
    
    public NetworkVariable<int> networkEnemyType = new NetworkVariable<int>();
    public bool propertieschecked = false;
    
    [Header("Dodging")]
    public float dodgeSpeed;
    public float dodgeCooldown;
    public float dodgeTimer = 0f;
    
    [Header("Testing")] //Remove after model impliementation
    public Material groundEnemyMaterial;
    public Material flyingEnemyMaterial;

    private void Awake()
    {
        // enemyType = enemyAiScriptable[networkEnemyType.Value];
        // Debug.Log(enemyType.name);

        
        InitializeStates();
        
        SearchTarget();
        
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        ChangeState(chaseState);


        
    }
    
    private void Update()
    {
        
        if (networkEnemyType.Value != Array.IndexOf(enemyAiScriptable, enemyType))
        {
            enemyType = enemyAiScriptable[networkEnemyType.Value];
            Debug.Log(enemyType.name);
        }

        if (!propertieschecked)
        {
            if (IsServer)
            {
                health.Value = enemyType.health;
                damage.Value = enemyType.damage;
                sightRange.Value = enemyType.sightRange;
                attackRange.Value = enemyType.attackRange;
                speed.Value = enemyType.speed;
                increaseSpeedOnGettingAttacked.Value = enemyType.increaseSpeedOnGettingAttacked;
                enemyType = enemyAiScriptable[networkEnemyType.Value];
            }
        
            if (!enemyType.isGroundEnemy)
            {
                if (agent.agentTypeID != -1372625422)
                {
                    agent.agentTypeID = -1372625422;
                }
                if (agent.baseOffset != baseOffset)
                {
                    agent.baseOffset = baseOffset;
                }
            }
        
            
            propertieschecked = true;
        }
        
        EnemyTypeCondition();
        
        _currentState?.Update();
        
        dodgeTimer += Time.deltaTime;
    }

    private void InitializeStates()
    {
        chaseState = new ChaseState(this);
        attackState = new AttackState(this);
        dodgeState = new DodgeState(this);
        deathState = new DeathState(this);
    }
    
    public void ChangeState(BaseState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        health.Value -= damage;

        // Additional logic when the enemy is hit
        if (health.Value > 0 && dodgeTimer >= dodgeCooldown)
        {
            ChangeState(dodgeState);
        }

        if (health.Value <= 0)
        {
            DestroyEnemyClientRpc();
        }
    }
    
    [ClientRpc]
    private void DestroyEnemyClientRpc()
    {
        Debug.Log("Dead");
        ChangeState(deathState);
    }
    
    public void AssignTarget(string targetType)
    {
        bool targetFound = false;
        while (!targetFound)
        {
            Transform selectedTarget = _targets[UnityEngine.Random.Range(0, _targets.Count)];

            if (selectedTarget.CompareTag(targetType))
            {
                if (targetType == "Player")
                {
                    PlayerStats targetPlayerStats = selectedTarget.GetComponentInChildren<PlayerStats>();
                    if (targetPlayerStats != null && !targetPlayerStats.isDead.Value)
                    {
                        target = selectedTarget;
                        playerStats = targetPlayerStats;
                        targetFound = true;
                    }
                }
                else if (targetType == "PermanentPart")
                {
                    target = selectedTarget;
                    targetFound = true;
                }
            }
        }
        
        if (target == null)
        {
            target = _targets[Random.Range(0, _targets.Count)];
        }
    }
    
    private void SearchTarget()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            // Check if the game object is a root object
            if (playerObject.transform.parent == null)
            {
                _targets.Add(playerObject.transform);
            }
        }

        GameObject[] partObjects = GameObject.FindGameObjectsWithTag("PermanentPart");
        foreach (GameObject partObject in partObjects)
        {
            _targets.Add(partObject.transform);
        }

        if (_targets.Count > 0)
        {
            target = _targets[Random.Range(0, _targets.Count)];
        }

        if (target != null && target.gameObject.CompareTag("Player"))
        {
            playerStats = target.GetComponentInChildren<PlayerStats>();
        }
    }

    private void EnemyTypeCondition()
    {
        /*// Check for boss type enemy
        if (enemyType.isBossEnemy)
        {
            bool hasIncreasedSpeed = false;
            bool hasIncreasedTransform = false;

            // Remove this once models are implemented
            if (!hasIncreasedTransform)
            {
                transform.localScale = new Vector3(3, 3, 3);
                hasIncreasedTransform = true;
            }

            Debug.Log("Boss Enemy");
            float healthThreshold = enemyType.health * bossHealthThreshold;

            if (health.Value <= healthThreshold && !hasIncreasedSpeed)
            {
                agent.speed += increaseBossSpeed;
                Debug.Log(agent.speed + " " + increaseBossSpeed);
                hasIncreasedSpeed = true;
            }
        }*/

        bool hasChangedMaterial = false;
        if (!hasChangedMaterial)
        {
            if (enemyType.isGroundEnemy)
            {
                Renderer renderer = GetComponent<Renderer>();
                renderer.material = groundEnemyMaterial;
            }
            else
            {
                Renderer renderer = GetComponent<Renderer>();
                renderer.material = flyingEnemyMaterial;
            }

            hasChangedMaterial = true;
        }
    }

    
}
