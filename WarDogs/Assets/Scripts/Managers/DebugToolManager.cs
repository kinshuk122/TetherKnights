using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace IObserver
{
    public interface IDebugObserver
    {
        void OnChanged(string identifier, int values);
    }
}

public class DebugToolManager : NetworkBehaviour, IObserver.IDebugObserver
{
    [Header("Text")]
    public Text waveText;
    public Text enemiesToSpawnText;
    public Text activeSpawnPointText;
    public Text enemiesAliveText;
    public Text permanentPartsText;
    public Text gameLengthText;
    public Text hasWaveStartedText;

    [Header("Referneces")] 
    public GameObject DebugToolCanvas;
    public TextMeshProUGUI gameOverTextControl;
    private float elapsedTime = 0f;

    void Start()
    {
        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.AddObserver(this);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (GameManager.instance != null)
            {
                permanentPartsText.text = "Permanent Parts: " + GameManager.instance.permanentParts.Value.ToString();

                if (GameManager.instance.hasWaveStarted.Value)
                {
                    elapsedTime += Time.deltaTime;
                    gameLengthText.text = "Game Length: " + elapsedTime.ToString("F2");
                    hasWaveStartedText.text = "Wave Started: " + GameManager.instance.hasWaveStarted.Value.ToString();
                }
                else
                {
                    hasWaveStartedText.text = "Wave Started: " + GameManager.instance.hasWaveStarted.Value.ToString();
                }
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                DebugToolCanvas.SetActive(!DebugToolCanvas.activeSelf);
            }
        
            if (Input.GetKeyDown(KeyCode.F2))
            {
                GameManager.instance.hasWaveStarted.Value = !GameManager.instance.hasWaveStarted.Value;
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                WaveSpawner.instance.enemiesToSpawn += 10;
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                WaveSpawner.instance.enemiesToSpawn -= 10;
                if (WaveSpawner.instance.enemiesToSpawn < 0)
                {
                    WaveSpawner.instance.enemiesToSpawn = 0;
                }
            }
            
            if(Input.GetKeyDown(KeyCode.F5))
            {
                gameOverTextControl.enabled = !gameOverTextControl.enabled;            }
        }
    }

    
    public void OnChanged(string identifier, int values)
    {
        if(identifier == "Wave")
        {
            waveText.text = "Wave: " + values.ToString();
        }
        else if (identifier == "ActiveSpawnPoint")
        {
            activeSpawnPointText.text = "Active Spawn Points: " + values.ToString();
        }
        else if(identifier == "EnemiesAlive")
        {
            enemiesAliveText.text = "Enemies Alive: " + values.ToString();
        }
        else if (identifier == "EnemiesToSpawn")
        {
            enemiesToSpawnText.text = "Enemies To Spawn: " + values.ToString();
        }
    }

    void OnDestroy()
    {
        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.RemoveObserver(this);
        }
    }
}
