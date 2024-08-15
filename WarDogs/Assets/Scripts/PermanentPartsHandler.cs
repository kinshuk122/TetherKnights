using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PermanentPartsHandler : MonoBehaviour
{
    public float health;
    public bool isDestroyed = false;
    
    [Header("Repairing")]
    public float repairAmount;
    public bool isRepairing;
    public PlayerInput playerInput;
    
    [Header("Audio Reference")]
    public AudioClip repairAudio;
    public AudioClip destroyAudio;
    public AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(health <= 0)
        {
            isDestroyed = true;
            AudioSource.PlayClipAtPoint(destroyAudio, transform.position);
            this.gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player is here");
            playerInput = other.GetComponentInParent<PlayerInput>();

            if (playerInput != null)
            {
                if (playerInput.actions["Repair"].IsPressed())
                {
                    isRepairing = true;
                    health += repairAmount * Time.deltaTime;
                    
                    if (!audioSource.isPlaying)
                    {
                        audioSource.clip = repairAudio;
                        audioSource.Play();
                    }
                }
                else
                {
                    if (audioSource != null)
                    {
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
            isRepairing = false;
        }
    }
}
