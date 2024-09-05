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
    public NetworkVariable<int> permanentParts = new NetworkVariable<int>();
    public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>();

    [Header("Text Reference")] 
    public TextMeshProUGUI gameOverText;
    
    private void Awake()
    {
        gameOverText.enabled = false;
        instance = this;
    }

    void Update()
    {
        if(currentAlivePlayers == 0 && hasWaveStarted.Value && IsServer || permanentParts.Value <= 0 && IsServer)
        {
            hasWaveStarted.Value = false;
            isGameOver.Value = true;
        }
        
        if(isGameOver.Value)
        {
            gameOverText.enabled = true;
        }
    }
}
