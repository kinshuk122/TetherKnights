using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    
    [Header("Players Settings")]
    public int currentPlayer;
    public int currentAlivePlayers;

    [Header("Wave Settings")] 
    public bool hasWaveStarted = false; //This is getting changed in pauseMenu
    
    private void Awake()
    {
        instance = this;
        currentAlivePlayers = currentPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentAlivePlayers <= 0)
        {
        }
    }
}
