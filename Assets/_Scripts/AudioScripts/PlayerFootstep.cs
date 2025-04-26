using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootstep : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float baseStepInterval = 0.4f;
    [SerializeField] private float stepRandomness = 0.15f;
    [SerializeField] private Vector2 volumeRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private AudioClip[] footstepClips;

    private AudioSource audioSource;
    private NetworkedCharacterMovement movement;
    private float stepTimer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        movement = GetComponent<NetworkedCharacterMovement>();
    }

    private void Update()
    {
        if (!CanPlayFootstep()) return;

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();

            // Randomize next step interval slightly
            float randomInterval = baseStepInterval * Random.Range(1f - stepRandomness, 1f + stepRandomness);
            stepTimer = randomInterval;
        }
    }

    private bool CanPlayFootstep()
    {
        if (movement == null) return false;

        bool isGrounded = movement.IsGrounded;
        bool isMoving = movement.MovementVelocity.sqrMagnitude > 0.1f;

        var health = GetComponent<EntityHealth>();
        bool isDowned = health != null && health.isDowned.Value;

        return !isDowned && isGrounded && isMoving;
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        // Pick random clip
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];

        // Randomize pitch and volume
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = Random.Range(volumeRange.x, volumeRange.y);

        audioSource.PlayOneShot(clip);
    }
}
