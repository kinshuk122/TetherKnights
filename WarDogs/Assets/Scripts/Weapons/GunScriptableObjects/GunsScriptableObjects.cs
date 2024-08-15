using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Guns", menuName = "Gun", order = 1)]
public class GunsScriptableObjects : ScriptableObject
{
    [Header("Guns General Stats")]
    public int damage;
    public float fireRate;
    public float spread;
    public float range;
    public float reloadTime;
    public float timeBetweenShots;
    public int magazineSize; 
    public int bulletsPerTap;
    public bool allowButtonHold;
    public GameObject gunPrefab;
    public bool bulletObject;
    
    [Header("Recoil")]
    public float recoil;
    public float recoilResetSpeed;
    public float recoilRotationSpeed;
    
    [Header("CameraShake")]
    public float camShakeMagnitude;
    public float camShakeDuration;
    
    [Header("Graphics")]
    public Material material;
    public AnimationCurve animationCurve;
    public float duration;
    
    // public Gradient minColor;
    // public Gradient maxColor;
    public Color emissionColor;
}
