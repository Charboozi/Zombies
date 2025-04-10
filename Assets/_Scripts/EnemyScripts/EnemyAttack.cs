using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public float attackRange = 2f; 

    [Header("Audio")]
    public AudioClip hitSound;

    private AudioSource audioSource;
    private RandomSoundPlayer randomSoundPlayer;

    private Transform target;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    private NavMeshAgent agent;
    private IEnemyAnimationHandler enemyAnimation;

    private bool isDead = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<IEnemyAnimationHandler>() ?? NullEnemyAnimationHandler.Instance;

        audioSource = GetComponent<AudioSource>();
        randomSoundPlayer = GetComponent<RandomSoundPlayer>();

        if (TryGetComponent(out EnemyDeathHandler deathHandler))
        {
            deathHandler.OnEnemyDeath += HandleDeath;
        }
    }

    private void HandleDeath()
    {
        isDead = true;
        StopAttack();
    }

    // Called by TargetScanner when a target is acquired or remains in range.
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Called when the target is confirmed to be in attack range.
    public void TargetInRange()
    {
        if (isDead) return;

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

        if (agent.enabled)
            agent.isStopped = true;

        if (enemyAnimation.HasAttackAnimation)
        {
            // ✅ Play animation if available
            enemyAnimation.TriggerAttack();
            PlayAttackAnimationClientRpc();

            // Failsafe: ensure we resume movement even if animation event fails
            CancelInvoke(nameof(OnAttackAnimationComplete));
            Invoke(nameof(OnAttackAnimationComplete), 2.5f); // Adjust this based on your animation length
        }
        else
        {
            // ✅ Fallback: attack directly at interval without animation
            TryDoDamage(); // Immediately do damage

            // Resume movement after cooldown
            Invoke(nameof(OnAttackAnimationComplete), attackCooldown);
        }
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
                PlayHitSoundClientRpc();

                health.TakeDamageServerRpc(attackDamage);
            }
        }
        else
        {
            //Debug.Log("Attack missed: target out of range");
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