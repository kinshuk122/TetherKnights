using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PoolManager : NetworkBehaviour
{
    public static PoolManager instance;
    
    [Header("BulletsTrail")]
    public int bulletTrainAmount;
    public List<GameObject> bulletTrailPool;
    public GameObject bulletTrailPrefab;
    public GameObject bulletTrailContainer;

    [Header("Bullet Object")]
    public int bulletObjectAmount;
    public List<GameObject> bulletObjectPool;
    public GameObject bulletObjectPrefab;
    public GameObject bulletObjectContainer;
    
    [Header("Enemy Bullet Object")]
    public int enemyBulletObjectAmount;
    public List<GameObject> enemyBulletObjectPool;
    public GameObject enemyBulletObjectPrefab;
    public GameObject enemyBulletObjectContainer;
    
    [Header("Enemy")]
    public int enemyAmount;
    public List<GameObject> enemyPool;
    public GameObject enemyPrefab;
    public GameObject enemyContainer;
    
    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        while (!NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }
        
        if (IsServer)
        {
            BulletTrailPooling();
            BulletObjectPooling();
            EnemyBulletObjectPooling();
            EnemyPooling();
        }
    }
    
    #region Bullet Trail Pooling

    public GameObject GetPooledBulletsTrails()
    {
        for (int i = 0; i < bulletTrainAmount; i++)
        {
            if (!bulletTrailPool[i].activeInHierarchy)
            {
                bulletTrailPool[i].SetActive(true);
                if (IsServer)
                {
                    NotifyClientsToActivateObjectClientRpc(bulletTrailPool[i].GetComponent<NetworkObject>().NetworkObjectId);
                }
                return bulletTrailPool[i];
            }
        }

        GameObject tmp = null;
        if (IsServer)
        {
            tmp = Instantiate(bulletTrailPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(bulletTrailContainer.transform);
            bulletTrailPool.Add(tmp);
            bulletTrainAmount++;
        }
        return tmp;
    }

    private void BulletTrailPooling()
    {
        bulletTrailPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < bulletTrainAmount; i++)
        {
            tmp = Instantiate(bulletTrailPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(bulletTrailContainer.transform);
            tmp.SetActive(false);
            bulletTrailPool.Add(tmp);
        }
    }
    
    [ClientRpc]
    private void NotifyClientsToActivateObjectClientRpc(ulong objectId)
    {
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;
        obj.SetActive(true);
    }

    #endregion

    #region Bullet Object Pooling

    public GameObject GetPooledBulletObject()
    {
        for (int i = 0; i < bulletObjectAmount; i++)
        {
            if (!bulletObjectPool[i].activeInHierarchy)
            {
                return bulletObjectPool[i];
            }
        }

        GameObject tmp = null;
        if (IsServer)
        {
            tmp = Instantiate(bulletObjectPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(bulletObjectContainer.transform);
            bulletObjectPool.Add(tmp);
            bulletObjectAmount++;
        }
        return tmp;
    }

    private void BulletObjectPooling()
    {
        bulletObjectPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < bulletObjectAmount; i++)
        {
            tmp = Instantiate(bulletObjectPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(bulletObjectContainer.transform);
            tmp.SetActive(false);
            bulletObjectPool.Add(tmp);
        }
    }

    #endregion

    #region Enemy Bullet Object Pooling

    public GameObject GetPooledEnemyBulletObject()
    {
        for (int i = 0; i < enemyBulletObjectAmount; i++)
        {
            if (!enemyBulletObjectPool[i].activeInHierarchy)
            {
                return enemyBulletObjectPool[i];
            }
        }

        GameObject tmp = null;
        if (IsServer)
        {
            tmp = Instantiate(enemyBulletObjectPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(enemyBulletObjectContainer.transform);
            enemyBulletObjectPool.Add(tmp);
            enemyBulletObjectAmount++;
        }
        return tmp;
    }

    private void EnemyBulletObjectPooling()
    {
        enemyBulletObjectPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < enemyBulletObjectAmount; i++)
        {
            tmp = Instantiate(enemyBulletObjectPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(enemyBulletObjectContainer.transform);
            tmp.SetActive(false);
            enemyBulletObjectPool.Add(tmp);
        }
    }

    #endregion

    #region Enemy

    public GameObject GetPooledEnemy()
    {
        for (int i = 0; i < enemyAmount; i++)
        {
            if (!enemyPool[i].activeInHierarchy)
            {
                enemyPool[i].SetActive(true);
                if (IsServer)
                {
                    NotifyClientsToActivateEnemyClientRpc(enemyPool[i].GetComponent<NetworkObject>().NetworkObjectId);
                }
                return enemyPool[i];
            }
        }

        GameObject tmp = null;
        if (IsServer)
        {
            tmp = Instantiate(enemyPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(enemyContainer.transform);
            enemyPool.Add(tmp);
            enemyAmount++;
        }
        return tmp;
    }

    private void EnemyPooling()
    {
        enemyPool = new List<GameObject>();
        GameObject tmp;
        for (int i = 0; i < enemyAmount; i++)
        {
            tmp = Instantiate(enemyPrefab);
            tmp.GetComponent<NetworkObject>().Spawn(true);
            tmp.transform.SetParent(enemyContainer.transform);
            tmp.SetActive(false);
            enemyPool.Add(tmp);
        }
    }
    
    [ClientRpc]
    private void NotifyClientsToActivateEnemyClientRpc(ulong objectId)
    {
        var obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;
        obj.SetActive(true);
    }

    #endregion
}
