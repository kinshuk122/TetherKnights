using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    
    public static GameManager instance;
    
    [Header("Players Settings")]
    public int currentAlivePlayers;

    [Header("Wave Settings")] 
    public NetworkVariable<bool> hasWaveStarted = new NetworkVariable<bool>();

    [Header("Text Reference")] 
    public TextMeshProUGUI gameOverText;
    
    private void Awake()
    {
        gameOverText.enabled = false;
        instance = this;
    }

    void Update()
    {
        if(currentAlivePlayers == 0 && hasWaveStarted.Value)
        {
            hasWaveStarted.Value = false;
            gameOverText.enabled = true;
        }
    }
}
