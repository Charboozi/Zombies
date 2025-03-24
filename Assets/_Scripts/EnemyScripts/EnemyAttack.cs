using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class EnemyAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    public float attackRate = 1.5f;
    public int attackDamage = 10;
    public float attackRange = 2f;

    [SerializeField] private Animator animator;

    private bool isAttacking = false;
    private NavMeshAgent agent;
    private Transform target;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void StartAttack()
    {
        if (!isAttacking && IsServer && target != null)
        {
            StartCoroutine(AttackLoop());
        }
    }

    private IEnumerator AttackLoop()
    {
        isAttacking = true;

        if (agent != null) agent.isStopped = true;
        if (animator != null) animator.SetBool("isAttacking", true);

        while (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Wait for animation wind-up
            yield return new WaitForSeconds(0.6f);

            if (target != null && Vector3.Distance(transform.position, target.position) <= attackRange)
            {
                if (target.TryGetComponent(out EntityHealth health))
                {
                    health.TakeDamageServerRpc(attackDamage);
                }
            }

            yield return new WaitForSeconds(attackRate);
        }

        // Reset state
        isAttacking = false;
        if (agent != null) agent.isStopped = false;
        if (animator != null) animator.SetBool("isAttacking", false);
    }
}
