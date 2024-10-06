using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using IObserver;

public class WaveSpawner : NetworkBehaviour
{
    public static WaveSpawner instance;
    private List<IDebugObserver> observers = new List<IDebugObserver>();

    [Header("WaveSystem")] 
    public GameObject enemyPrefab;
    private GameObject spawnPoint;
    private bool waveIncremented = false;
    private bool randomiseSpawn;
    private int breakLength; // in seconds
    public float percentageOfEnemiesToTargetPermanentPart = 0.2f;
    private NetworkVariable<int> selectedEnemyTypeId = new NetworkVariable<int>();
    
    [Header("NotControlledValues - Change to Private in future")]
    public int maxEnemies;
    public int enemiesAlive = 0;
    public int enemiesToSpawn;
    public int wave = 1;
    public List<GameObject> activeSpawnPoints = new List<GameObject>();

    [Header("Boss Wave Settings")]
    public EnemyAIScriptableObject bossEnemy; //Change to array if more bosses are introduced
    public int reduceSpawnByDivision = 2; //divide the spawn by this number to reduce the number of enemies spawned
    
    [Header("EnemySpawning")]
    public int additionalMaxEnemies = 2; //Gamedesigners vars
    public int minEnemies; //Spawn next wave when only this many enemies are left
    public int additionalEnemiesPerWave = 2;
    public int additionalEnemiesPerBreaches = 2;
    
    [Header("EnemyAI")]
    public EnemyAIScriptableObject[] enemyAiScriptable; //0:Assault, 1:Crawler, 2:Sniper
    private int totalEnemyTypes;
    
    [Header("Audio Reference")]
    public AudioClip waveStartAudio;
    private AudioSource audioSource;
    private bool hasAudioPlayed = false;

    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
        totalEnemyTypes = enemyAiScriptable.Length;
    }

    void Update() 
    {
        if (!IsOwner || !GameManager.instance.hasWaveStarted.Value)
        {
            return;
        }

        if (GameManager.instance.hasWaveStarted.Value && !hasAudioPlayed)
        {
            audioSource.PlayOneShot(waveStartAudio);
            hasAudioPlayed = true;
        }
        else if (!GameManager.instance.hasWaveStarted.Value)
        {
            hasAudioPlayed = false;
        }
        
        if (enemiesAlive <= minEnemies)
        {
            if (!waveIncremented)
            {
                wave++;
                waveIncremented = true;
                NotifyChanged("Wave",wave); //Observer Pattern
            }

            if (wave % 10 == 0 && activeSpawnPoints.Count >= 12) //if active spawn points are more than 50% boss wave 
            {
                randomiseSpawn = false;
                SpawnEnemies(enemiesToSpawn / reduceSpawnByDivision , bossEnemy);
                return;
            }
            else if (wave % 5 == 0) //Particular Enemy Wave
            {
                randomiseSpawn = false;
                StartCoroutine(BreakSpawnEnemies(breakLength, enemyAiScriptable[Random.Range(0, totalEnemyTypes)]));
                SpawnEnemies(enemiesToSpawn, enemyAiScriptable[Random.Range(0, totalEnemyTypes)]);
                return;
            }
            else //Default Wave
            {
                randomiseSpawn = true;
                SpawnEnemies(enemiesToSpawn, enemyAiScriptable[0]);
                return;
            }
        }
        else
        {
            waveIncremented = false;
        }
        
        NotifyChanged("EnemiesAlive", enemiesAlive); //Observer Pattern
        NotifyChanged("EnemiesToSpawn", enemiesToSpawn); //Observer Pattern
    }
    
    // Observer management
    public void AddObserver(IDebugObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(IDebugObserver observer)
    {
        observers.Remove(observer);
    }

    private void NotifyChanged(string identifier, int changedValue)
    {
        foreach (var observer in observers)
        {
            observer.OnChanged(identifier, changedValue);
        }
    }
    
    private void SpawnEnemies(int numberOfEnemies, EnemyAIScriptableObject enemyType)
    {
        // maxEnemies = maxEnemies + additionalMaxEnemies;
        enemiesToSpawn = ((minEnemies + (wave * additionalEnemiesPerWave) + (activeSpawnPoints.Count * additionalEnemiesPerBreaches)) - enemiesAlive);
        
        if (enemiesToSpawn >= maxEnemies)
        {
            enemiesToSpawn = maxEnemies;
        }

        int permanentPartTargetCount = Mathf.CeilToInt(numberOfEnemies * percentageOfEnemiesToTargetPermanentPart);
        int playerTargetCount = numberOfEnemies - permanentPartTargetCount; 
        
        for (int i = 0; i < numberOfEnemies; i++)
        {
            if (activeSpawnPoints.Count > 0)
            {
                spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Count)];
                NotifyChanged("ActiveSpawnPoint", activeSpawnPoints.Count); //Observer Pattern, its called when a spawn point is used
                
                if (IsServer)
                {
                    if (randomiseSpawn)
                    {
                        selectedEnemyTypeId.Value = Random.Range(0, totalEnemyTypes);
                    }
                    else
                    {
                        selectedEnemyTypeId.Value = Array.IndexOf(enemyAiScriptable, enemyType);
                    }

                    enemyType = enemyAiScriptable[selectedEnemyTypeId.Value]; // All clients use the same enemy type

                    GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);

                    NetworkObject networkObject = enemyInstance.GetComponent<NetworkObject>();
                    if (networkObject != null)
                    {
                        networkObject.Spawn();
                    }
                    
                    EnemyAi enemyAiScript = enemyInstance.GetComponent<EnemyAi>();
                    
                    enemyAiScript.networkEnemyType.Value = Array.IndexOf(enemyAiScriptable, enemyType);
                    
                    if (permanentPartTargetCount > 0)
                    {
                        enemyAiScript.AssignTarget("PermanentPart");
                        permanentPartTargetCount--;
                    }
                    else
                    {
                        enemyAiScript.AssignTarget("Player");
                        playerTargetCount--;
                    }

                    enemyInstance.GetComponent<EnemyAi>().networkEnemyType.Value = Array.IndexOf(enemyAiScriptable, enemyType);
                    enemiesAlive++;
                }
            }
        }
    }
    
    private IEnumerator BreakSpawnEnemies(int duration, EnemyAIScriptableObject enemyType)
    {
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            if (IsServer)
            {
                selectedEnemyTypeId.Value = Random.Range(0, totalEnemyTypes);
            }

            enemyType = enemyAiScriptable[selectedEnemyTypeId.Value];

            if (enemiesAlive < minEnemies)
            {
                SpawnEnemies(minEnemies - enemiesAlive, enemyType);
            }

            yield return new WaitForSeconds(duration);
        }
    }
}
