using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    [Header("PlayerStats")] 
    public float maxHealth;
    public NetworkVariable<float> health = new NetworkVariable<float>();
    public NetworkVariable<bool> allowToHeal = new NetworkVariable<bool>();
    public NetworkVariable<float> respawnTimer = new NetworkVariable<float>();
    public NetworkVariable<float> respawnTimeIncrease = new NetworkVariable<float>();
    public float healingAmount;
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>();
    
    [Header("Healing")]
    public float secondsAfterHealAllowed;
    private float timeSinceLastDamage;
    
    private void Awake()
    {
        GameManager.instance.currentAlivePlayers++;
    }

    private void Update()
    {
        if (IsServer) 
        {
            timeSinceLastDamage += Time.deltaTime;
            
            if (timeSinceLastDamage >= secondsAfterHealAllowed && !isDead.Value)
            {
                allowToHeal.Value = true;
            }
            
            if (health.Value < maxHealth && allowToHeal.Value)
            {
                health.Value += Time.deltaTime * healingAmount;
            }

            if (health.Value <= 2 && !isDead.Value)
            {
                GameManager.instance.currentAlivePlayers--;
                isDead.Value = true;
                allowToHeal.Value = false;
                // fpsControllerScript.enabled = false;
                // gunsScript.enabled = false;
                
                respawnTimer.Value = respawnTimeIncrease.Value;
            }
            
            if (isDead.Value)
            {
                Debug.Log("Player is dead.");
                RespawnCountdown(); 
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage)
    {
        if (health.Value > 0)
        {
            health.Value -= damage;
            timeSinceLastDamage = 0f;
            allowToHeal.Value = false;

            if (health.Value <= 0)
            {
                health.Value = 0;
            }
        }
    }

    private void RespawnCountdown()
    {
        respawnTimer.Value -= Time.deltaTime;

        if (respawnTimer.Value <= 0)
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        if (isDead.Value)
        {
            GameManager.instance.currentAlivePlayers++;
            Debug.Log(GameManager.instance.currentAlivePlayers);
            isDead.Value= false;

            RespawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RespawnServerRpc()
    {
        // GameManager.instance.currentAlivePlayers++;
        isDead.Value = false;
        health.Value = maxHealth;
        allowToHeal.Value = true;
    }
    
    // [ClientRpc]
    // private void RespawnClientRpc()
    // {
    //     if (!IsLocalPlayer) return;
    //
    //     fpsControllerScript.enabled = true;
    //     gunsScript.enabled = true;
    //
    //     RespawnServerRpc();
    // }
    
}