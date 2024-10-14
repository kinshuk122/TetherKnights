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
    public NetworkVariable<NetworkObjectReference> inHandItem = new NetworkVariable<NetworkObjectReference>();
    public NetworkVariable<bool> isHolding = new NetworkVariable<bool>(); 
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
                RespawnCountdown(); 
            }
        }
        
        
        // Pickup and place logic
        if (hit.collider != null)
        {
            hit.collider.GetComponentInParent<Highlight>()?.ToggleHighlight(false);
        }
        
        if(Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out hit, hitRange, pickableLayerMask))
        {
            hit.collider.GetComponentInParent<Highlight>()?.ToggleHighlight(true);
            if (playerInput.actions["Pickup"].WasPressedThisFrame() && !isHolding.Value)
            {
                Pickup();
            }
        }
        
        if (playerInput.actions["Pickup"].WasReleasedThisFrame())
        {
            if (isHolding.Value)
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
                NetworkObject networkObject = hit.collider.GetComponentInParent<NetworkObject>();
                if (networkObject != null && networkObject.IsSpawned)
                {
                    if (IsServer || networkObject.IsOwner)
                    {
                        ExecutePickup(networkObject);
                    }
                    else
                    {
                        RequestPickupServerRpc(networkObject.NetworkObjectId);
                    }
                }
            }
        }
    }

    private void Drop()
    {
        if (inHandItem.Value.TryGet(out NetworkObject networkObject))
        {
            Vector3 dropPosition = playerCameraTransform.position + playerCameraTransform.forward * 2.0f;
            RaycastHit floorHit;
            if (Physics.Raycast(dropPosition, Vector3.down, out floorHit, Mathf.Infinity))
            {
                dropPosition.y = floorHit.point.y;
            }

            if (networkObject != null)
            {
                if (IsServer)
                {
                    Debug.Log("ExecuteDrop");
                    ExecuteDrop(networkObject, dropPosition);
                }
                else
                {
                    Debug.Log("RequestDropServerRpc");
                    RequestDropServerRpc(networkObject.NetworkObjectId, dropPosition);
                }
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
    
    
    //Pickup logic functions
    private void ExecutePickup(NetworkObject networkObject)
    {
        Rigidbody rb = networkObject.GetComponentInParent<Rigidbody>();
        inHandItem.Value = networkObject.gameObject;

        NetworkObject pickUpParentNetworkObject = pickUpParent.GetComponent<NetworkObject>();
        if (pickUpParentNetworkObject != null)
        {
            networkObject.TrySetParent(pickUpParentNetworkObject);
        }
        else
        {
            networkObject.transform.SetParent(pickUpParent.transform, true);
        }

        networkObject.transform.position = pickUpParent.position + new Vector3(0f, 0.20f, 1f);
        networkObject.transform.rotation = pickUpParent.rotation;
        isHolding.Value = true;
        Debug.Log(isHolding);
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong networkObjectId, ServerRpcParams rpcParams = default)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        ExecutePickup(networkObject);
    }
    
    private void ExecuteDrop(NetworkObject networkObject, Vector3 dropPosition)
    {
        Rigidbody rb = networkObject.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        networkObject.transform.position = dropPosition;
        networkObject.transform.SetParent(null);
        inHandItem.Value = new NetworkObjectReference(networkObject);
        isHolding.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDropServerRpc(ulong networkObjectId, Vector3 dropPosition, ServerRpcParams rpcParams = default)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        ExecuteDrop(networkObject, dropPosition);
    }
}