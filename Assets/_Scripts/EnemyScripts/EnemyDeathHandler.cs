using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(PowerupSpawner))]
[RequireComponent(typeof(EnemyAnimationHandler))]
public class EnemyDeathHandler : NetworkBehaviour, IKillable
{
    [Header("Death Settings")]
    public float destroyDelay = 10f;

    [SerializeField]private Animator animator;

    private NavMeshAgent agent;
    private CapsuleCollider capsule;
    private PowerupSpawner powerupSpawner;
    private EnemyAnimationHandler enemyAnimation;

    private bool isDead = false;

    public event Action OnEnemyDeath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        capsule = GetComponent<CapsuleCollider>();
        powerupSpawner = GetComponent<PowerupSpawner>();
        enemyAnimation = GetComponent<EnemyAnimationHandler>();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false; 
        }
        if (capsule != null) capsule.enabled = false;

        enemyAnimation.PlayDeath();

        powerupSpawner?.TrySpawn();

        StopAllCoroutines(); // Stop attacks, roaming, etc.
        StartCoroutine(DestroyAfterDelay());
        OnEnemyDeath?.Invoke();
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (IsServer)
        {
            NetworkObject netObj = GetComponent<NetworkObject>();
            if (netObj != null && netObj.IsSpawned)
                netObj.Despawn(true);

            Destroy(gameObject);
        }
    }
}
