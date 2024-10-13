using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Pickup&Place")] 
    [SerializeField] private LayerMask pickableLayerMask;
    [SerializeField] [Min(1)] private float hitRange;
    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform pickUpParent;
    [SerializeField] private GameObject inHandItem;
    public bool isHolding;
    PlayerInput playerInput;
    private RaycastHit hit;
    
    private void Awake()
    {
        GameManager.instance.currentAlivePlayers++;
        playerInput = GetComponentInParent<PlayerInput>();
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
                
                respawnTimer.Value = respawnTimeIncrease.Value;
            }
            
            if (isDead.Value)
            {
                Debug.Log("Player is dead.");
                RespawnCountdown(); 
            }
        }
        
        // Pickup and place logic
        if (hit.collider != null)
        {
            hit.collider.GetComponentInParent<Highlight>()?.ToggleHighlight(false);
        }
        
        Debug.DrawRay(playerCameraTransform.position, playerCameraTransform.forward * hitRange, Color.green);
        if(Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, hitRange, pickableLayerMask))
        {
            hit.collider.GetComponentInParent<Highlight>()?.ToggleHighlight(true);
            if (playerInput.actions["Pickup"].WasPressedThisFrame() && !isHolding)
            {
                Pickup();
            }
        }
        
        if (playerInput.actions["Pickup"].WasReleasedThisFrame())
        {
            if (isHolding)
            {
                Drop();
            }
        }
    }

    private void Pickup()
    {
        if(hit.collider != null)
        {
            if (((1 << hit.collider.gameObject.layer) & pickableLayerMask) != 0)
            {
                Rigidbody rb = hit.collider.GetComponentInParent<Rigidbody>();
                Debug.Log(hit.collider.name);
                inHandItem = hit.collider.transform.parent.gameObject;
                inHandItem.transform.SetParent(pickUpParent.transform,true);
                inHandItem.transform.localPosition = Vector3.zero;
                inHandItem.transform.localRotation = Quaternion.identity;
                isHolding = true;
                if(rb != null)
                {
                    rb.isKinematic = true;
                    Debug.Log("Picked up " + inHandItem.name);
                }
                return;
            }
        }
    }

    private void Drop()
    {
        Vector3 dropPosition = playerCameraTransform.position + playerCameraTransform.forward * 2.0f;
        RaycastHit floorHit;
        if (Physics.Raycast(dropPosition, Vector3.down, out floorHit, Mathf.Infinity))
        {
            dropPosition.y = floorHit.point.y;
        }

        Rigidbody rb = inHandItem.gameObject.GetComponentInParent<Rigidbody>();
        
        if(rb != null)
        {
            rb.isKinematic = false;
        }
        
        inHandItem.transform.position = dropPosition;
        inHandItem.transform.SetParent(null);
        inHandItem = null;
        isHolding = false;
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
    
}