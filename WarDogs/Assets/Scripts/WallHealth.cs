using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallHealth : NetworkBehaviour
{
    public NetworkVariable<float> health = new NetworkVariable<float>(30f);
    public float maxHealth;
    public NetworkVariable<float> timeCounter = new NetworkVariable<float>();
    private float time;
    private float damage;
    public GameObject spawnPoint;

    [Header("Colors")]
    public Material red;
    public Material blue;
    
    public enum MaterialState { Red, Blue }
    private MaterialState materialState;

    [Header("Repairing")]
    public NetworkVariable<float> repairAmount = new NetworkVariable<float>(5f);
    public NetworkVariable<bool> isRepairing = new NetworkVariable<bool>();
    private PlayerInput playerInput;
    public NetworkVariable<bool> broken = new NetworkVariable<bool>();
    private Coroutine repairCoroutine;

    [Header("Audio Reference")]
    public AudioClip repairAudio;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        time = Random.Range(0, 15);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = 30f;
            maxHealth = health.Value;
            repairAmount.Value = 5f;
        }
    }

    void Update()
    {
        if (!GameManager.instance.hasWaveStarted.Value)
        {
            return;
        }
        
        if (IsServer)
        {
            if (health.Value > maxHealth)
            {
                health.Value = maxHealth;
            }

            if (!isRepairing.Value && !broken.Value)
            {
                timeCounter.Value += Time.deltaTime;

                if (timeCounter.Value >= time)
                {
                    damage = Random.Range(0, 15);
                    health.Value -= damage;
                    time = Random.Range(0, 15);
                    timeCounter.Value = 0f;
                }
            }

            if (isRepairing.Value)
            {
                RepairWallServerRpc();
            }

            // Check and update material state
            UpdateMaterialState();
        }

        if (health.Value <= 0f)
        {
            broken.Value = true;
            spawnPoint.SetActive(true);
        }
        else
        {
            broken.Value = false;
            spawnPoint.SetActive(false);
        }
    }

    private void UpdateMaterialState()
    {
        if (health.Value <= 0f && materialState != MaterialState.Red)
        {
            materialState = MaterialState.Red;
            UpdateMaterialClientRpc(MaterialState.Red);
        }
        else if (health.Value > 0f && materialState != MaterialState.Blue)
        {
            materialState = MaterialState.Blue;
            UpdateMaterialClientRpc(MaterialState.Blue);
        }
    }

    [ClientRpc]
    private void UpdateMaterialClientRpc(MaterialState newState)
    {
        if (newState == MaterialState.Red)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = red;
        }
        else if (newState == MaterialState.Blue)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = blue;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInput = other.GetComponent<PlayerInput>();

            if (playerInput != null)
            {
                if (playerInput.actions["Repair"].IsPressed())
                {
                    if (IsClient)
                    {
                        RequestStartRepairingServerRpc();
                    }
                }
                else
                {
                    if (IsClient)
                    {
                        RequestStopRepairingServerRpc();
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
            RepairWallServerRpc();
            yield return new WaitForSeconds(0.1f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepairWallServerRpc()
    {
        health.Value += repairAmount.Value * 1f;

        UpdateMaterialState();
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
    public void RequestStopRepairingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (repairCoroutine != null)
        {
            StopCoroutine(repairCoroutine);
            repairCoroutine = null;
            isRepairing.Value = false;
            audioSource.Stop();
        }
    }
}
