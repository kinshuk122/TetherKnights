using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Wave Controller")]
    public WaveSpawner waveSpawner;

    [Header("Pause Menu")]
    private bool isPaused = false;
    public GameObject pauseMenu;

    private void Awake()
    {
        pauseMenu.SetActive(false);
    }

    void Update()
    {
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
        waveSpawner.enabled = true; //Add Breakable wall spawns as well, stop them from starting 
    }
}
