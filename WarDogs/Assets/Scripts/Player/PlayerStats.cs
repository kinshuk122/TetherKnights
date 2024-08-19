using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("PlayerStats")] 
    public float maxHealth;
    public float health = 120;
    public bool allowToHeal;
    public float damage;
    public float respawnTimer;
    public float respawnTimeIncrease;
    public bool isDead;
    
    [Header("References")]
    public FirstPersonController fpsControllerScript;
    public Guns gunsScript;

    private void Awake()
    {
        fpsControllerScript = GetComponentInParent<FirstPersonController>();
        GameManager.instance.currentAlivePlayers++;
    }

    private void Update()
    {
        if (health <= maxHealth && allowToHeal)
        {
            health += Time.deltaTime / 2;
        }

        if (health <= 0)
        {
            if (!isDead)
            {
                GameManager.instance.currentAlivePlayers--;
                isDead = true;
            }
            
            allowToHeal = false;
            Debug.Log("Dead");
            fpsControllerScript.enabled = false;
            gunsScript.enabled = false;
            Respawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet") && health >= -1f)
        {
            allowToHeal = false;
            StartCoroutine(HealthBool());
        }
    }
    
    private IEnumerator HealthBool()
    {
        yield return new WaitForSeconds(5f);
        allowToHeal = true;
    }

    private void Respawn()
    {
        respawnTimer -= Time.deltaTime;
        
        if(respawnTimer <= 0)
        {
            if (isDead)
            {
                GameManager.instance.currentAlivePlayers++;
                isDead = false;
            }
            
            fpsControllerScript.enabled = true;
            gunsScript.enabled = true;
            respawnTimer =+ respawnTimeIncrease;
            health = maxHealth;
            allowToHeal = true;
        }
    }
}
