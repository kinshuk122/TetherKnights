using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PermanentPartsHandler : NetworkBehaviour
{
    [Header("PermanentPart settings")]
    public NetworkVariable<float> health = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isDestroyed = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Server);
    public float maxHealth;
    
    [Header("Repairing")]
    public NetworkVariable<float> repairAmount = new NetworkVariable<float>();
    public NetworkVariable<bool> isRepairing = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Server);
    private PlayerInput playerInput;
    private Coroutine repairCoroutine;

    [Header("Audio Reference")]
    public AudioClip repairAudio;
    public AudioClip destroyAudio;
    private AudioSource audioSource;

    private void Awake()
    {
        repairAmount.Value = 5;
        health.Value = 1100;
        maxHealth = health.Value;
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (IsServer)
        {
            if (health.Value <= 0 && !isDestroyed.Value)
            {
                isDestroyed.Value = true;
                AudioSource.PlayClipAtPoint(destroyAudio, transform.position);
                this.gameObject.SetActive(false);
            }

            if (health.Value > 0 && isDestroyed.Value)
            {
                isDestroyed.Value = false;
                this.gameObject.SetActive(true);
            }

            if (isRepairing.Value)
            {
                RepairPartServerRpc();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInput = other.GetComponentInParent<PlayerInput>();

            if (playerInput != null)
            {
                if (playerInput.actions["Repair"].IsPressed())
                {
                    if (IsClient)
                    {
                        RequestStartRepairingServerRpc();
                    }

                    if (IsServer && repairCoroutine == null)
                    {
                        repairCoroutine = StartCoroutine(RepairOverTime());
                    }

                    if (IsServer && !audioSource.isPlaying)
                    {
                        audioSource.clip = repairAudio;
                        audioSource.Play();
                    }
                }
                else
                {
                    if (IsServer && repairCoroutine != null)
                    {
                        StopCoroutine(repairCoroutine);
                        repairCoroutine = null;
                        isRepairing.Value = false;
                        audioSource.Stop();
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInput = null;

            if (IsServer && repairCoroutine != null)
            {
                StopCoroutine(repairCoroutine);
                repairCoroutine = null;
                isRepairing.Value = false;
                audioSource.Stop();
            }
        }
    }

    private IEnumerator RepairOverTime()
    {
        isRepairing.Value = true;

        while (isRepairing.Value)
        {
            RepairPartServerRpc();
            yield return new WaitForSeconds(0.1f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepairPartServerRpc()
    {
        health.Value += repairAmount.Value * 0.1f;

        if (health.Value > maxHealth)
        {
            health.Value = maxHealth;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestStartRepairingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (health.Value < maxHealth)
        {
            isRepairing.Value = true;
            if (repairCoroutine == null)
            {
                repairCoroutine = StartCoroutine(RepairOverTime());
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestTakeDamageServerRpc(float damage)
    {
        TakeDamage(damage);
    }
    
    public void TakeDamage(float damage)
    {
        health.Value -= damage;
        if (health.Value <= 0)
        {
            isDestroyed.Value = true;
            this.gameObject.SetActive(false);
        }
    }
}
