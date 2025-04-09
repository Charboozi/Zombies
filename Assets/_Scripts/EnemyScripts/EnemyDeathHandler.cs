using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.AI;
using System;

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider), typeof(PowerupSpawner))]
public class EnemyDeathHandler : NetworkBehaviour, IKillable
{
    [Header("Death Settings")]
    public float destroyDelay = 10f;

    [SerializeField]private Animator animator;

    [Header("Scripts to Disable on Death")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private NavMeshAgent agent;
    private CapsuleCollider capsule;
    private PowerupSpawner powerupSpawner;
    private IEnemyAnimationHandler enemyAnimation;

    public AudioClip[] deathClips; // Assign in Inspector
    private AudioSource audioSource;
    private RandomSoundPlayer randomSoundPlayer;
    private int lastPlayedClipIndex = -1;

    private bool isDead = false;

    public event Action OnEnemyDeath;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        capsule = GetComponent<CapsuleCollider>();
        powerupSpawner = GetComponent<PowerupSpawner>();

        enemyAnimation = GetComponent<IEnemyAnimationHandler>() ?? NullEnemyAnimationHandler.Instance;

        audioSource = GetComponent<AudioSource>();
        randomSoundPlayer = GetComponent<RandomSoundPlayer>();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        agent.ResetPath();
        agent.enabled = false;
        capsule.enabled = false;

        DisableScripts();

        enemyAnimation.PlayDeath();
        randomSoundPlayer?.StopSounds();

        // Pick a random clip and store index
        if (deathClips.Length > 0)
        {
            lastPlayedClipIndex = UnityEngine.Random.Range(0, deathClips.Length);
            PlayDeathClipClientRpc(lastPlayedClipIndex);
        }

        powerupSpawner?.TrySpawn();
        StopAllCoroutines();
        StartCoroutine(DestroyAfterDelay());
        OnEnemyDeath?.Invoke();
    }

    private void DisableScripts()
    {
        foreach (var script in scriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }
    }

    [ClientRpc]
    private void PlayDeathClipClientRpc(int clipIndex)
    {
        if (clipIndex < 0 || clipIndex >= deathClips.Length || audioSource == null)
            return;

        audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(deathClips[clipIndex]);
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
