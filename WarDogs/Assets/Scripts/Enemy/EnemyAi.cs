using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAi : NetworkBehaviour
{
    //Increase speed of some enemies on chse state using navmesh agent component 
    
    [Header("General")]
    public EnemyAIScriptableObject enemyType;
    public NavMeshAgent agent;
    public float baseOffset; //Change it to 4 once camera works perfectly
    public Transform target;
    public List<Transform> targets;
    private PlayerStats playerStats;
    public GameObject enemyBulletGo;
    public bool isActive;
    public NetworkVariable<int> networkEnemyType = new NetworkVariable<int>();
    public EnemyAIScriptableObject[] enemyAiScriptable;
    
    [Header("For Instantiating")]
    public GameObject enemyBullet;
    
    [Header("Testing")] //Remove after model impliementation
    public Material groundEnemyMaterial;
    public Material flyingEnemyMaterial;

    [Header("Attack")] 
    public GameObject firePoint;
    public float dodgeSpeed;
    private bool isDodging = false;
    public float dodgeCooldown;
    private float dodgeTimer = 0f;
    private Vector3 dodgePosition;
    private bool alreadyAttacked;
    
    [Header("About States")]
    public bool playerInAttackRange;
    public bool playerInLineOfSight;
    
    [Header("Enemy Type Conditions")] 
    public float increaseBossSpeed;
    public float bossHealthThreshold = 0.5f;
    
    [Header("ScriptableObject References")] 
    public NetworkVariable<float> networkHealth = new NetworkVariable<float>();
    public NetworkVariable<float> damage = new NetworkVariable<float>();
    public NetworkVariable<float> sightRange = new NetworkVariable<float>();
    public NetworkVariable<float> attackRange = new NetworkVariable<float>();
    public NetworkVariable<float> speed = new NetworkVariable<float>();
    public NetworkVariable<float> increaseSpeedOnGettingAttacked = new NetworkVariable<float>();
    private bool areAllPropertiesEqual = false;
    
    [Header("Audio Refernece")]
    public AudioClip permanentPartHitAudio;
    public AudioClip hitAudio;
    private AudioSource audioSource;

    private void Awake()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObject in playerObjects)
        {
            // Check if the game object is a root object
            if (playerObject.transform.parent == null)
            {
                targets.Add(playerObject.transform);
            }
        }

        GameObject[] partObjects = GameObject.FindGameObjectsWithTag("PermanentPart");
        foreach (GameObject partObject in partObjects)
        {
            targets.Add(partObject.transform);
        }

        if (targets.Count > 0)
        {
            target = targets[Random.Range(0, targets.Count)];
        }

        if (target != null && target.gameObject.CompareTag("Player"))
        {
            playerStats = target.GetComponentInChildren<PlayerStats>();
        }

        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (networkEnemyType.Value != Array.IndexOf(enemyAiScriptable, enemyType))
        {
            enemyType = enemyAiScriptable[networkEnemyType.Value];
        }

        if (!areAllPropertiesEqual && IsServer)
        {
            ArePropertiesEqual();
        }

        playerInAttackRange = false;
        playerInLineOfSight = false;

        dodgeTimer += Time.deltaTime;
        if (isDodging)
        {
            transform.position = Vector3.MoveTowards(transform.position, dodgePosition, dodgeSpeed * Time.deltaTime);
            if (transform.position == dodgePosition)
            {
                isDodging = false;
            }
        }

        Collider[] collidersInAttackRange = Physics.OverlapSphere(transform.position, attackRange.Value);

        Vector3 raycastOrigin = firePoint != null ? firePoint.transform.position : transform.position + Vector3.up;
        foreach (var collider in collidersInAttackRange)
        {
            if (collider.transform == target)
            {
                playerInAttackRange = true;

                Vector3 directionToPlayer = (target.position - raycastOrigin).normalized;
                RaycastHit hit;
                int layerMask = LayerMask.GetMask("Default", "Player"); 

                if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, attackRange.Value, layerMask))
                {
                    if (hit.transform.CompareTag("Player") || hit.transform.CompareTag("PermanentPart"))
                    {
                        playerInLineOfSight = true;
                    }
                }

                break;
            }
        }

        if (!playerInAttackRange || !playerInLineOfSight)
        {
            ChasePlayer();
        }

        if (target.CompareTag("Player"))
        {
            if (playerStats != null && playerStats.isDead.Value)
            {
                ChasePlayer();
            }
        }

        if (!target.gameObject.activeInHierarchy)
        {
            ChasePlayer();
        }

        if (playerInAttackRange && playerInLineOfSight)
        {
            AttackPlayer();
        }

        EnemyTypeCondition();
    }

    public void AssignTarget(string targetType)
    {
        bool targetFound = false;
        while (!targetFound)
        {
            Transform selectedTarget = targets[UnityEngine.Random.Range(0, targets.Count)];

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
            target = targets[Random.Range(0, targets.Count)];
        }
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
            playerInAttackRange = false;

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
            playerInAttackRange = false;

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

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(target);
        transform.LookAt(firePoint.transform);

        if (!alreadyAttacked)
        {
            // Attack Code

            // enemyBulletGo = PoolManager.instance.GetPooledEnemyBulletObject();
            // if (enemyBulletGo != null)
            // {
            //     enemyBulletGo.transform.position = this.transform.position;
            //     enemyBulletGo.transform.rotation = this.transform.rotation;
            //     enemyBulletGo.SetActive(true);
            // }
            
            enemyBulletGo = Instantiate(enemyBullet);
            if (enemyBulletGo != null)
            {
                enemyBulletGo.transform.position = firePoint.transform.position;
                enemyBulletGo.transform.rotation = firePoint.transform.rotation;
                enemyBulletGo.SetActive(true);
            }

            Rigidbody rb = enemyBulletGo.GetComponent<Rigidbody>();
            enemyBulletGo.transform.position = firePoint.transform.position;
            Vector3 throwDirection = (target.position - firePoint.transform.position).normalized;
            rb.AddForce(throwDirection * 500f, ForceMode.Impulse);

            RaycastHit hit;
            if (Physics.Raycast(firePoint.transform.position, throwDirection, out hit, sightRange.Value))
            {
                PlayerStats playerStats = hit.collider.GetComponent<PlayerStats>();
                if (playerStats != null && playerStats.health != null)
                {
                    playerStats.TakeDamageServerRpc(damage.Value);
                    audioSource.PlayOneShot(hitAudio, 0.75f);
                }

                if (hit.collider.CompareTag("PermanentPart"))
                {
                    hit.collider.GetComponent<PermanentPartsHandler>().RequestTakeDamageServerRpc(damage.Value);
                    audioSource.PlayOneShot(permanentPartHitAudio, 1f);

                    if (hit.collider.GetComponent<PermanentPartsHandler>().isDestroyed.Value)
                    {
                        hit.collider.gameObject.SetActive(false);
                        ChasePlayer();
                    }
                }
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), enemyType.timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        // Reduce the enemy's health on the server
        networkHealth.Value -= damage;

        // Additional logic when the enemy is hit
        if (networkHealth.Value > 0 && dodgeTimer >= dodgeCooldown)
        {
            // Dodge the next attack
            int dodgeDirection = (Random.Range(0, 2) * 2 - 1) * 2; // Generates either -2 or 2
            Vector3 towardsPlayer = (target.position - transform.position).normalized;
            Vector3 dodgeVector = Vector3.Cross(towardsPlayer, Vector3.up) * dodgeDirection;
            dodgePosition = transform.position + dodgeVector;
            isDodging = true;
            dodgeTimer = 0f;
        }

        if (networkHealth.Value <= 0)
        {
            DestroyEnemyClientRpc();
        }
    }
    
    [ClientRpc]
    public void TakeDamageClientRpc(int damage)
    {
        if (IsOwner)
        {
            TakeDamageServerRpc(damage);
        }
    }

    [ClientRpc]
    private void DestroyEnemyClientRpc()
    {
        Debug.Log("Dead");
        WaveSpawner.instance.enemiesAlive--;
        this.gameObject.SetActive(false);
    }

    private void EnemyTypeCondition()
    {
        // Check for boss type enemy
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

            if (networkHealth.Value <= healthThreshold && !hasIncreasedSpeed)
            {
                agent.speed += increaseBossSpeed;
                Debug.Log(agent.speed + " " + increaseBossSpeed);
                hasIncreasedSpeed = true;
            }
        }

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

    private (bool, string) ArePropertiesEqual()
    {
        if (damage.Value != enemyType.damage)
        {
            damage.Value = enemyType.damage;
            return (false, "damage");
        }
        if (sightRange.Value != enemyType.sightRange)
        {
            sightRange.Value = enemyType.sightRange;
            return (false, "sightRange");
        }
        if (attackRange.Value != enemyType.attackRange)
        {
            attackRange.Value = enemyType.attackRange;
            return (false, "attackRange");
        }
        if (speed.Value != enemyType.speed)
        {
            speed.Value = enemyType.speed;
            agent.speed = speed.Value;
            return (false, "speed");
        }
        if (increaseSpeedOnGettingAttacked.Value != enemyType.increaseSightOnGettingAttacked)
        {
            increaseSpeedOnGettingAttacked.Value = enemyType.increaseSightOnGettingAttacked;
            return (false, "increaseSpeedOnGettingAttacked");
        }
        if (agent.speed != speed.Value)
        {
            agent.speed = speed.Value;
            return (false, "agent.speed");
        }
        if(networkHealth.Value != enemyType.health)
        {
            networkHealth.Value = enemyType.health;
            return (false, "networkHealth.Value");
        }
        if (!enemyType.isGroundEnemy)
        {
            if (agent.agentTypeID != -1372625422)
            {
                agent.agentTypeID = -1372625422;
                return (false, "agent.agentTypeID");
            }
            if (agent.baseOffset != baseOffset)
            {
                agent.baseOffset = baseOffset;
                return (false, "agent.baseOffset");
            }
        }
        areAllPropertiesEqual = true;

        return (true, "");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange.Value);
        
    }
}
