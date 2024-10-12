using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Guns : NetworkBehaviour
{
    [Header("Gun Stats scriptable object")]
    public GunsScriptableObjects currentGunStat;
    public GunsScriptableObjects[] gunsArray; //M4, LMG, Revolver, SMG
    
    [Header("Guns General Stats")]
    public int bulletsLeft, bulletsShot;
    public Transform gunTransform;
    private Vector3 direction;
    private float x;
    private float y;
    // public GameObject bulletTrailPrefab;
    public Slider gunReloadBar;
    public float colorIntensity;
    public float throwSpeed;
    public Rigidbody rb;
    
    [Header("Bools")]
    bool shooting, readyToShoot, reloading;

    
    [Header("References")]
    public Camera fpsCam;
    public Transform attackPoint;
    public LayerMask whatIsEnemy;

    [Header("Graphics")]
    public GameObject bulletHoleGraphic;
    
    [Header("For Instantiating")]
    public GameObject bulletPrefab;

    [Header("Player")] 
    public PlayerStats playerStats;
    
   private void Awake()
    {
        fpsCam = Camera.main;
        gunReloadBar = GameObject.Find("ReloadSlider").GetComponent<Slider>();
        bulletsLeft = currentGunStat.magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (playerStats.isDead.Value)
        {
            return;
        }

        MyInput();

        direction = fpsCam.transform.forward + new Vector3(x, y, 0);
        Quaternion rotation = Quaternion.LookRotation(direction);
        gunTransform.rotation = rotation;

        gunReloadBar.maxValue = currentGunStat.magazineSize;
        gunReloadBar.value = bulletsLeft;
    }

    private void MyInput()
    {
        shooting = currentGunStat.allowButtonHold ? Input.GetKey(KeyCode.Mouse0) : Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < currentGunStat.magazineSize && !reloading)
        {
            StartCoroutine(Reload());
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = currentGunStat.bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        x = Random.Range(-currentGunStat.spread, currentGunStat.spread);
        y = Random.Range(-currentGunStat.spread, currentGunStat.spread);
        direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        RaycastHit rayHit;

        if (Physics.Raycast(attackPoint.position, direction, out rayHit, currentGunStat.range, whatIsEnemy))
        {
            if (rayHit.collider.CompareTag("Enemy"))
            {
                if (IsClient)
                {
                    // Request the server to handle the damage
                    rayHit.collider.GetComponent<EnemyStateMachine>().TakeDamageServerRpc(currentGunStat.damage);
                }
            }
        }

        RequestShootServerRpc(attackPoint.position, direction, rayHit.point, rayHit.collider != null);

        bulletsLeft--;
        bulletsShot--;

        Invoke("ResetShot", currentGunStat.fireRate);

        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", currentGunStat.fireRate);
        }
    }

    [ServerRpc]
    private void RequestShootServerRpc(Vector3 attackPosition, Vector3 direction, Vector3 hitPoint, bool hitSomething)
    {
        //GameObject trailInstance = Instantiate(bulletPrefab, position, rotation);
        
        GameObject trailInstance = Instantiate(bulletPrefab, attackPosition, Quaternion.LookRotation(direction));

        if (trailInstance != null)
        {
            NetworkObject networkObject = trailInstance.GetComponent<NetworkObject>();

            if (IsServer)
            {
                networkObject.Spawn();
            }
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private IEnumerator Reload()
    {
        reloading = true;
        float elapsedTime = 0f;

        while (elapsedTime < currentGunStat.reloadTime)
        {
            gunReloadBar.value = Mathf.Lerp(0, currentGunStat.magazineSize, elapsedTime / currentGunStat.reloadTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        ReloadFinished();
    }

    private void ReloadFinished()
    {
        bulletsLeft = currentGunStat.magazineSize;
        reloading = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.color = Color.red;
        Vector3 startPos = fpsCam.transform.position;
        Vector3 direction = fpsCam.transform.forward;
        Gizmos.DrawLine(startPos, startPos + direction * currentGunStat.range);
    }
}
