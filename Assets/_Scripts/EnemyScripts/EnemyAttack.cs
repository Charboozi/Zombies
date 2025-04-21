using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetScanner))]
public class EnemyAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public float rotationSpeed = 10f;

    [Header("Audio")]
    public AudioClip hitSound;

    private AudioSource audioSource;
    private RandomSoundPlayer randomSoundPlayer;
    private Collider enemyCollider;
    private NavMeshAgent agent;
    private IEnemyAnimationHandler enemyAnimation;

    private Transform target;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private bool isDead = false;

    public event Action<Transform> OnTargetHit;

    private TargetScanner scanner;

    private void Awake()
    {
        agent           = GetComponent<NavMeshAgent>();
        scanner         = GetComponent<TargetScanner>();
        enemyCollider   = GetComponent<Collider>();
        audioSource     = GetComponent<AudioSource>();
        randomSoundPlayer = GetComponent<RandomSoundPlayer>();
        enemyAnimation  = GetComponent<IEnemyAnimationHandler>() 
                          ?? NullEnemyAnimationHandler.Instance;

        if (TryGetComponent(out EnemyDeathHandler deathHandler))
        {
            deathHandler.OnEnemyDeath += HandleDeath;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // ✅ Keep rotating toward target even while attacking
        if (target != null && isAttacking)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0f; // Ignore vertical rotation
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleDeath()
    {
        isDead = true;
        StopAttack();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void TargetInRange()
    {
        if (isDead || isAttacking || target == null || cooldownTimer > 0f) return;

        StartAttack();
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    private void StartAttack()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        if (agent.enabled)
            agent.isStopped = true;

        if (enemyAnimation.HasAttackAnimation)
        {
            enemyAnimation.TriggerAttack();
            PlayAttackAnimationClientRpc();

            CancelInvoke(nameof(OnAttackAnimationComplete));
            Invoke(nameof(OnAttackAnimationComplete), 2.5f);
        }
        else
        {
            TryDoDamage();
            Invoke(nameof(OnAttackAnimationComplete), attackCooldown);
        }
    }

    public void TryDoDamage()
    {
        if (!IsServer || target == null)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > scanner.inRangeThreshold)
            return;

        bool hitConfirmed = false;

        // Dynamic origin point based on collider bounds
        Vector3 origin = enemyCollider.bounds.center;
        origin.y = enemyCollider.bounds.max.y - 0.1f; // slightly below top of collider
        origin += transform.forward * (enemyCollider.bounds.extents.z + 0.2f); // forward offset

        Vector3 direction = (target.position - origin).normalized;

        // First: try SphereCast ✅ (bigger radius to catch enemies of different sizes)
        if (Physics.SphereCast(origin, 0.5f, direction, out RaycastHit sphereHit, scanner.inRangeThreshold))
        {
            Debug.DrawRay(origin, direction * sphereHit.distance, Color.green, 1f);

            if (sphereHit.transform == target)
            {
                hitConfirmed = true;
            }
        }

        // Fallback: if spherecast missed, try OverlapSphere at target position ✅
        if (!hitConfirmed)
        {
            Collider[] overlaps = Physics.OverlapSphere(target.position, 0.7f);
            foreach (var collider in overlaps)
            {
                if (collider.transform == target)
                {
                    // Check line of sight
                    if (Physics.Raycast(origin, direction, out RaycastHit losHit, scanner.inRangeThreshold))
                    {
                        Debug.DrawRay(origin, direction * losHit.distance, Color.yellow, 1f);

                        if (losHit.transform == target)
                        {
                            hitConfirmed = true;
                            break;
                        }
                    }
                }
            }
        }

        if (hitConfirmed)
        {
            if (target.TryGetComponent(out EntityHealth health))
            {
                // 2) Fire the event instead of calling TryRemoveRandomEquipmentFromLocalPlayer
                OnTargetHit?.Invoke(target);

                PlayHitSoundClientRpc();
                health.TakeDamageServerRpc(attackDamage);
            }
        }
    }

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

    [ClientRpc]
    private void PlayHitSoundClientRpc()
    {
        if (randomSoundPlayer != null)
            randomSoundPlayer.StopSounds();

        if (audioSource.isPlaying)
            audioSource.Stop();

        if (hitSound != null)
            audioSource.PlayOneShot(hitSound);
    }
}
