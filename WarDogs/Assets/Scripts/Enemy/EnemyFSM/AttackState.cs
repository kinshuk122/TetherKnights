using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AttackState : BaseState
{
    private NavMeshAgent agent;
    private Transform target;
    private GameObject firePoint;
    private float sightRange;
    private float damage;
    private float attackRange;

    [Header("Bools")]
    private bool alreadyAttacked;
    
    public AttackState(EnemyStateMachine StateMachine) : base(StateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        model.stateName = "Attack";

        agent = model.agent;
        target = model.target;
        firePoint = model.firePoint;
        sightRange = model.sightRange.Value;
        damage = model.damage.Value;
        attackRange = model.attackRange.Value;
    }

    public override void Update()
    {
        base.Update();

        if (model.enemyType.isSuicideBomber)
        {
            Explode();
        }
        else
        {
            AttackPlayer();
        }
        
        float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
        if (target == null || distanceToTarget > attackRange)
        {
            model.ChangeState(model.chaseState);
            model.playerInLineOfSight = false;
        }

        if(model.playerStats != null)
        {
            if (model.playerStats.isDead.Value)
            {
                model.ChangeState(model.chaseState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private void AttackPlayer()
    {
        agent.SetDestination(agent.transform.position);
        agent.transform.LookAt(target);
        agent.transform.LookAt(firePoint.transform);

        if (!alreadyAttacked)
        {
            // enemyBulletGo = Instantiate(enemyBullet);
            // if (enemyBulletGo != null)
            // {
            //     enemyBulletGo.transform.position = firePoint.transform.position;
            //     enemyBulletGo.transform.rotation = firePoint.transform.rotation;
            //     enemyBulletGo.SetActive(true);
            // }

            // Rigidbody rb = enemyBulletGo.GetComponent<Rigidbody>();
            // enemyBulletGo.transform.position = firePoint.transform.position;
            Vector3 throwDirection = (target.position - firePoint.transform.position).normalized;
            // rb.AddForce(throwDirection * 500f, ForceMode.Impulse);

            
            RaycastHit hit;
            if (Physics.Raycast(firePoint.transform.position, throwDirection, out hit, sightRange))
            {
                PlayerStats playerStats = hit.collider.GetComponent<PlayerStats>();
                if (playerStats != null && playerStats.health != null)
                {
                    playerStats.TakeDamageServerRpc(damage);
                    // audioSource.PlayOneShot(hitAudio, 0.75f);
                }

                if (hit.collider.CompareTag("PermanentPart"))
                {
                    hit.collider.GetComponent<PermanentPartsHandler>().RequestTakeDamageServerRpc(damage);
                    // audioSource.PlayOneShot(permanentPartHitAudio, 1f);
                    if (hit.collider.GetComponent<PermanentPartsHandler>().isDestroyed.Value)
                    {
                        hit.collider.gameObject.SetActive(false);
                    }
                }
            }

            alreadyAttacked = true;
            model.StartCoroutine(ResetAttack());
        }
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(agent.transform.position, model.enemyType.explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = agent.gameObject.AddComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(model.enemyType.explosionDamage, agent.transform.position, model.enemyType.explosionRadius);
            }

            float distance = Vector3.Distance(agent.transform.position, nearbyObject.transform.position);
            float damage = Mathf.Lerp(model.enemyType.explosionDamage, 0, distance / model.enemyType.explosionRadius);

            PlayerStats playerStats = nearbyObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamageServerRpc(damage);
                Debug.Log($"Damaged Player: {nearbyObject.name} for {damage} damage.");
            }

            PermanentPartsHandler part = nearbyObject.GetComponent<PermanentPartsHandler>();
            if (part != null)
            {
                part.RequestTakeDamageServerRpc(damage);
            }

            WallHealth wallHealth = nearbyObject.GetComponentInChildren<WallHealth>();
            if (wallHealth != null)
            {
                wallHealth.health.Value -= damage;
            }
        }
        model.ChangeState(model.deathState);
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(model.enemyType.fireRate);
        alreadyAttacked = false;
    }
}
