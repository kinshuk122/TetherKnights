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
    public Transform player;
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
    public bool playerInSightRange;
    public bool playerInAttackRange;

    [Header("Enemy Type Conditions")] 
    public float increaseBossSpeed;
    public float bossHealthThreshold = 0.5f;
    
    [Header("ScriptableObject References")] 
    public NetworkVariable<float> networkHealth = new NetworkVariable<float>(120);
    private float damage;
    private float sightRange;
    private float attackRange;
    private float speed;
    private float increaseSpeedOnGettingAttacked;
    
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
            player = targets[Random.Range(0, targets.Count)];
        }

        if (player != null && player.gameObject.CompareTag("Player"))
        {
            playerStats = player.GetComponentInChildren<PlayerStats>();
        }

        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (IsServer)
        {
            // Initialize Network Variables on the server
            networkHealth.Value = enemyType.health;
        }

        damage = enemyType.damage;
        sightRange = enemyType.sightRange;
        attackRange = enemyType.attackRange;
        speed = enemyType.speed;
        increaseSpeedOnGettingAttacked = enemyType.increaseSightOnGettingAttacked;
        agent.speed = speed;

        if (!enemyType.isGroundEnemy)
        {
            agent.agentTypeID = -1372625422; //Flying Enemy ID found from Inspector Debug
            agent.baseOffset = baseOffset;
        }
    }

    private void Update()
    {
        if (networkEnemyType.Value != Array.IndexOf(enemyAiScriptable, enemyType))
        {
            enemyType = enemyAiScriptable[networkEnemyType.Value];
        }
        
        // Dodge
        dodgeTimer += Time.deltaTime;
        if (isDodging)
        {
            transform.position = Vector3.MoveTowards(transform.position, dodgePosition, dodgeSpeed * Time.deltaTime);
            if (transform.position == dodgePosition)
            {
                isDodging = false;
            }
        }

        Collider[] collidersInSightRange = Physics.OverlapSphere(transform.position, sightRange);
        Collider[] collidersInAttackRange = Physics.OverlapSphere(transform.position, attackRange);

        if (!playerInSightRange)
        {
            foreach (var collider in collidersInSightRange)
            {
                if (collider.transform == player)
                {
                    playerInSightRange = true;
                    break;
                }
            }
        }

        if (!playerInAttackRange)
        {
            foreach (var collider in collidersInAttackRange)
            {
                if (collider.transform == player)
                {
                    playerInAttackRange = true;
                    break;
                }
            }
        }

        if (!playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }

        if (player.CompareTag("Player"))
        {
            if (playerStats != null && playerStats.isDead)
            {
                ChasePlayer();
            }
        }

        if (!player.gameObject.activeInHierarchy)
        {
            ChasePlayer();
        }

        if (playerInAttackRange && playerInSightRange)
        {
            AttackPlayer();
        }

        EnemyTypeCondition();
    }

    public void AssignTarget(string targetType)
    {
        if (targetType == "PermanentPart")
        {
            foreach (Transform target in targets)
            {
                if (target.CompareTag("PermanentPart"))
                {
                    Debug.Log("Permanent Part Targeted");
                    player = target; 
                    break;
                }
            }
        }
        else if (targetType == "Player")
        {
            foreach (Transform target in targets)
            {
                if (target.CompareTag("Player") && !target.GetComponentInChildren<PlayerStats>().isDead)
                {
                    player = target;
                    playerStats = player.GetComponentInChildren<PlayerStats>();
                    break;
                }
            }
        }
        
        if (player == null)
        {
            player = targets[Random.Range(0, targets.Count)];
        }
    }
    
    private void ChasePlayer()
    {
        if (playerStats != null && playerStats.isDead)
        {
            playerInSightRange = false;
            playerInAttackRange = false;

            for (int i = targets.Count - 1; i >= 0; i--)
            {
                PlayerStats otherPlayerStats = targets[i].GetComponentInChildren<PlayerStats>();
                if (otherPlayerStats != null && !otherPlayerStats.isDead)
                {
                    player = targets[i];
                    playerStats = otherPlayerStats;
                    break;
                }
            }
        }

        if (!player.gameObject.activeInHierarchy)
        {
            playerInSightRange = false;
            playerInAttackRange = false;

            isActive = false;
            while (!isActive)
            {
                player = targets[Random.Range(0, targets.Count)];
                if (player.gameObject.activeInHierarchy)
                {
                    isActive = true;
                }
            }
        }

        if (player != null)
        {
            agent.SetDestination(player.position);
            isActive = false;
        }
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(player);
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
                enemyBulletGo.transform.position = this.transform.position;
                enemyBulletGo.transform.rotation = this.transform.rotation;
                enemyBulletGo.SetActive(true);
            }

            Rigidbody rb = enemyBulletGo.GetComponent<Rigidbody>();
            enemyBulletGo.transform.position = firePoint.transform.position;
            Vector3 throwDirection = (player.position - firePoint.transform.position).normalized;
            rb.AddForce(throwDirection * enemyType.throwSpeed, ForceMode.Impulse);

            RaycastHit hit;
            if (Physics.Raycast(firePoint.transform.position, throwDirection, out hit, sightRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<PlayerStats>().health -= damage;
                    audioSource.PlayOneShot(hitAudio, 0.75f);
                }

                if (hit.collider.CompareTag("PermanentPart"))
                {
                    hit.collider.GetComponent<PermanentPartsHandler>().RequestTakeDamageServerRpc(damage);
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
            Vector3 towardsPlayer = (player.position - transform.position).normalized;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
