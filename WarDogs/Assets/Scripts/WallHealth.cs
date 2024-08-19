using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallHealth : NetworkBehaviour
{
    public NetworkVariable<float> health = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    public float maxHealth;
    private Material wallMat;
    public NetworkVariable<float> timeCounter = new NetworkVariable<float>(writePerm: NetworkVariableWritePermission.Server);
    private float time;
    private float damage;
    public GameObject spawnPoint;

    [Header("Repairing")]
    public NetworkVariable<float> repairAmount = new NetworkVariable<float>();
    public NetworkVariable<bool> isRepairing = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Server);
    private PlayerInput playerInput;
    public NetworkVariable<bool> broken = new NetworkVariable<bool>(false, writePerm: NetworkVariableWritePermission.Server);
    private Coroutine repairCoroutine;

    [Header("Audio Reference")]
    public AudioClip repairAudio;
    private AudioSource audioSource;

    private void Awake()
    {
        health.Value = 10;
        repairAmount.Value = 5;
        maxHealth = health.Value;
        wallMat = GetComponent<Renderer>().material;
        audioSource = GetComponent<AudioSource>();
        time = Random.Range(0, 15);
    }

    void Update()
    {
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
        }

        if (health.Value <= 0f)
        {
            wallMat.color = Color.red;
            broken.Value = true;
            spawnPoint.SetActive(true);
        }
        else
        {
            wallMat.color = Color.blue;
            broken.Value = false;
            spawnPoint.SetActive(false);
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
            RepairWallServerRpc();
            yield return new WaitForSeconds(0.1f);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepairWallServerRpc()
    {
        health.Value += repairAmount.Value * 0.1f;
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
}
