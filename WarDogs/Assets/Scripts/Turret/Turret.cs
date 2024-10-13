using System;
using UnityEngine;
using System.Linq;

namespace Turrets
{
   public class Turret : MonoBehaviour
   {
      [Header("Turret Stats")] 
      public float health;
      public float fireRate;
      public float range;
      public float damage;
      
      [Header("Turret Setup")]
      public Transform target;
      public Transform head;
      public LayerMask enemyLayer;

      private float lastShotTime;

      private void Update()
      {
         FindTarget();
         
         if(target == null) return;
         
         float distanceToTarget = Vector3.Distance(target.position, transform.position);
         if(distanceToTarget <= range)
         {
            head.LookAt(target);
            if (Time.time >= lastShotTime + fireRate)
            {
               Shoot();
               lastShotTime = Time.time;
            }
         }
         
         if(health <= 0)
         {
            Destroy(gameObject);
         }
      }
      
      private void FindTarget()
      {
         Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, range, enemyLayer);
         if (enemiesInRange.Length > 0)
         {
            Collider closestEnemy = enemiesInRange.OrderBy(e => Vector3.Distance(transform.position, e.transform.position)).First();
            
            target = closestEnemy.transform;
         }
         else
         {
            target = null;
         }
      }

      private void Shoot()
      {
         if (target == null) return;

         RaycastHit hit;
         if (Physics.Raycast(head.position, head.forward, out hit, range, enemyLayer))
         {
            if (hit.transform.CompareTag("Enemy"))
            {
               hit.collider.GetComponent<EnemyStateMachine>().TakeDamageServerRpc(damage);
            }
         }
      }
   }
}