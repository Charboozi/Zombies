using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent), typeof(EnemyAnimationHandler))]
public class EnemyAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;  // The maximum distance at which the attack can hit

    private Transform target;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    private NavMeshAgent agent;
    private EnemyAnimationHandler enemyAnimation;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<EnemyAnimationHandler>();
    }

    // Called by TargetScanner when a target is acquired or remains in range.
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Called when the target is confirmed to be in attack range.
    public void TargetInRange()
    {
        // Only try to start an attack if we're not already attacking,
        // a target exists, and the cooldown has expired.
        if (!isAttacking && target != null && cooldownTimer <= 0f)
        {
            StartAttack();
        }
    }

    // Called when the target is lost or goes out of a reasonable range.
    public void StopAttack()
    {
        isAttacking = false;
        // Optionally, you can also reset cooldown here or keep it running.
    }

    private void Update()
    {
        if (!IsServer)
            return;

        // Decrease cooldown timer over time.
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;
        Debug.Log("Start Attacking!!!!!!!");

        if (agent.enabled)
            agent.isStopped = true;

        // Play the attack animation on the server.
        enemyAnimation.TriggerAttack();
        // Sync the attack animation on clients.
        PlayAttackAnimationClientRpc();

        // ⛑ Failsafe: ensure we resume movement even if animation event fails
        CancelInvoke(nameof(OnAttackAnimationComplete)); // cancel if already pending
        Invoke(nameof(OnAttackAnimationComplete), 2.5f); // fire after 2.5s if needed
    }

    // This method is triggered by an animation event at the moment of impact.
    public void TryDoDamage()
    {
        if (!IsServer || target == null)
            return;

        // Check if the target is still within attack range.
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange)
        {
            if (target.TryGetComponent(out EntityHealth health))
            {
                health.TakeDamageServerRpc(attackDamage);
            }
        }
        else
        {
            Debug.Log("Attack missed: target out of range");
        }
    }

    // This method is triggered via an animation event at the end of the attack.
    public void OnAttackAnimationComplete()
    {
        CancelInvoke(nameof(OnAttackAnimationComplete));

        isAttacking = false;
        if (agent.enabled)
            agent.isStopped = false;
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        enemyAnimation.TriggerAttack();
    }
}
