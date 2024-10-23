using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateBreaches : MonoBehaviour
{
    [SerializeField] private int totalBreaches;
    [SerializeField] private float breachThresholdPercentage = 0.7f;
    [SerializeField] private bool hasAdjustedHealth = false;

    private void Update()
    {
        if (WaveSpawner.instance != null)
        {
            //TODO: Change the random range of time and damage on wllHealth script instead
            float percentage = (float)WaveSpawner.instance.activeSpawnPoints.Count / totalBreaches;
            if (percentage > breachThresholdPercentage && !hasAdjustedHealth)
            {
                // wallHealthScript.health.Value += increasedBreachHealth;
                hasAdjustedHealth = true;
            }
            else if (percentage <= breachThresholdPercentage && hasAdjustedHealth)
            {
                // wallHealthScript.health.Value -= increasedBreachHealth;
                hasAdjustedHealth = false;
            }
        }
    }

    private void OnEnable()
    {
        if (WaveSpawner.instance != null)
        {
            WaveSpawner.instance.activeSpawnPoints.Add(this.gameObject);
        }
    }

    private void OnDisable()
    {
        WaveSpawner.instance.activeSpawnPoints.Remove(this.gameObject);
    }
}
