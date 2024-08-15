using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class WallHealth : MonoBehaviour
{
    public float health;
    private float maxHealth;
    private Material wallMat;
    public float timeCounter;
    private float time;
    private float damage;
    public GameObject spawnPoint;

    [Header("Repairing")]
    public float repairAmount;
    private bool isRepairing;
    private PlayerInput playerInput;
    
    [Header("Audio Reference")]
    public AudioClip repairAudio;
    private AudioSource audioSource;
    
    //TODO: Prone to error active the reduction of wallHealth only if wave system is active
    private void Awake()
    {
        maxHealth = health;
        wallMat = GetComponent<Renderer>().material;
        audioSource = GetComponent<AudioSource>();
        time = Random.Range(0, 15);
    }
    
    void Update()
    {
        if(health > maxHealth)
        {
            health = maxHealth;
        }
        
        if (!isRepairing)
        {
            timeCounter += Time.deltaTime;

            if (timeCounter >= time)
            {
                damage = Random.Range(0, 15);
                health -= damage;
                time = Random.Range(0, 15);
                timeCounter = 0f;
            }
        }
        
        if (health <= 0f)
        {
            wallMat.color = Color.red;
            spawnPoint.SetActive(true);
        }
        else
        {
            wallMat.color = Color.blue;
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
                    audioSource.Stop();
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
