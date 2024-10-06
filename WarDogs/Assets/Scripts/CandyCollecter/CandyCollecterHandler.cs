using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CandyCollecterHandler : NetworkBehaviour
{
    public static CandyCollecterHandler instance;
    
    [Header("Candy Collecter Settings")]
    public NetworkVariable<int> playerCandyCount = new NetworkVariable<int>();
    public NetworkVariable<int> machineCandyCount = new NetworkVariable<int>();
    public NetworkVariable<int> candyIncrementAmount = new NetworkVariable<int>();
    public TextMeshProUGUI candyCountText;
    public float collectionSpeedInSecs;
    private bool canCandyIncrement = false;
    private float timer = 0.0f;
    
    [Header("Player")]
    private PlayerInput playerInput;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        candyCountText.text = "Candy Amount: " + playerCandyCount.Value.ToString();

        if (IsServer && GameManager.instance.hasWaveStarted.Value)
        {
            timer += Time.deltaTime;
            if (timer >= collectionSpeedInSecs)
            {
                canCandyIncrement = true;
                timer = 0.0f;
            }
        
            if (canCandyIncrement)
            {
                machineCandyCount.Value += candyIncrementAmount.Value;
                canCandyIncrement = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInput = other.GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                NetworkBehaviour networkBehaviour = playerInput.GetComponent<NetworkBehaviour>();

                if (networkBehaviour.OwnerClientId == NetworkManager.Singleton.LocalClientId)
                {
                    if (playerInput.actions["Interact"].IsPressed())
                    {
                        if (IsClient)
                        {
                            CollectCandyServerRPC();
                        }
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectCandyServerRPC()
    {
        playerCandyCount.Value += machineCandyCount.Value;
        machineCandyCount.Value = 0;
    }
}
