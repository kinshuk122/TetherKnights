using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : NetworkBehaviour
{
    [Header("Wave Controller")]
    public WaveSpawner waveSpawner;
    public Button startWavesButton;
    
    [Header("Pause Menu")]
    private bool isPaused = false;
    public GameObject pauseMenu;

    private void Awake()
    {
        pauseMenu.SetActive(false);
    }

    void Update()
    {
        if (!IsOwner)
        {
            startWavesButton.enabled = false;
            startWavesButton.gameObject.SetActive(false);
        }
        else
        {
            startWavesButton.enabled = true;
            startWavesButton.gameObject.SetActive(true);
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape pressed");
            isPaused = !isPaused;
            pauseMenu.SetActive(isPaused);
        }
        
        if (isPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
    
    
    public void StartWaves()
    {
        StartWavesServerRpc();
        GameManager.instance.hasWaveStarted = true;
    }
    
    [ServerRpc]
    public void StartWavesServerRpc()
    {
        waveSpawner.enabled = true;
        StartWavesClientRpc();
    }

    [ClientRpc]
    public void StartWavesClientRpc()
    {
        waveSpawner.enabled = true;
    }
}
