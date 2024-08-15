using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "EnemyScriptableObject", order = 2)]
public class EnemyAIScriptableObject : ScriptableObject
{
    [Header("About Enemy")]
    public float health;
    public float damage;
    // public GameObject enemyPrefab;
    public float speed;
    public float increaseSpeedOnGettingAttacked;
    
    [Header("EnemyTypeBool")]
    public bool isGroundEnemy;
    public bool isBossEnemy;
    
    [Header("Attack")]
    public float timeBetweenAttacks;
    public float throwSpeed;
    public GameObject enemyBullet;

    [Header("About States")]
    public float sightRange;
    public float attackRange;
    public float increaseSightOnGettingAttacked;

}
