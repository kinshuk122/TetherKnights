using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    
    [Header("Players Settings")]
    public int currentAlivePlayers;

    [Header("Wave Settings")] 
    public bool hasWaveStarted = false;

    [Header("Text Reference")] 
    public TextMeshProUGUI gameOverText;
    
    private void Awake()
    {
        gameOverText.enabled = false;
        instance = this;
    }

    void Update()
    {
        if(currentAlivePlayers == 0 && hasWaveStarted)
        {
            hasWaveStarted = false;
            gameOverText.enabled = true;
        }
    }
}
