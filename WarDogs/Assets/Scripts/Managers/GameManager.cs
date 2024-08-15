using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    
    public int currentPlayer;
    public int currentAlivePlayers;

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
