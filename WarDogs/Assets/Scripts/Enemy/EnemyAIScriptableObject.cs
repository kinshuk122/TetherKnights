using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "EnemyScriptableObject", order = 2)]
public class EnemyAIScriptableObject : ScriptableObject
{
    [Header("About Enemy")]
    public float health;
    public float damage;
    public float speed;
    public float fireRate;

    [Header("EnemyTypeBool")]
    public bool isGroundEnemy;
    public bool isBossEnemy;
    public bool isSuicideBomber;
    
    [Header("About States")]
    public float sightRange;
    public float attackRange;
    
    [Header("GameObject Bullet")]
    public float throwSpeed;
    public GameObject enemyBullet;
    
    [Header("About Bomber Enemy")]
    public float explosionRadius;
    public float explosionDamage;
}
