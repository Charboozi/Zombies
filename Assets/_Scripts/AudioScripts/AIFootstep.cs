using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(AudioSource))]
public class AIFootstep : NetworkBehaviour
{
    [Header("Footstep Settings")]
    [SerializeField] private float baseStepInterval = 0.5f;
    [SerializeField] private float stepRandomness = 0.2f;
    [SerializeField] private float audibleRange = 20f;
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1f);
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioSource footstepAudioSource;

    private EnemyMovement movement;
    private float stepTimer;
    private Transform localPlayerTransform;

    public override void OnNetworkSpawn()
    {
        if (!IsClient)
        {
            enabled = false;
            return;
        }

        movement = GetComponent<EnemyMovement>();
        stepTimer = Random.Range(0f, baseStepInterval); // Desync footsteps

        // Cache the local player's transform (client only!)
        var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (playerObject != null)
        {
            localPlayerTransform = playerObject.transform;
        }
    }

    private void Update()
    {
        // Validate dependencies
        if (movement == null || footstepClips.Length == 0 || footstepAudioSource == null || localPlayerTransform == null)
            return;

        // Check distance to local player
        float distanceToPlayer = Vector3.Distance(transform.position, localPlayerTransform.position);
        if (distanceToPlayer > audibleRange)
            return; // Skip if too far away

        float speed = movement.GetSyncedSpeed();
        if (speed <= 0.1f)
            return; // Skip if enemy is not moving

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();

            // Dynamic step timing based on speed, with randomness
            float randomFactor = Random.Range(1f - stepRandomness, 1f + stepRandomness);
            float interval = baseStepInterval / Mathf.Max(speed, 0.1f) * randomFactor;
            stepTimer = interval;
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepAudioSource.pitch = Random.Range(0.95f, 1.05f);
        footstepAudioSource.volume = Random.Range(volumeRange.x, volumeRange.y);
        footstepAudioSource.PlayOneShot(clip);
    }
}
