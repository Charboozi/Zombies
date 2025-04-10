using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootstep : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float stepInterval = 0.3f;
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
            stepTimer = stepInterval;
        }
    }

    private bool CanPlayFootstep()
    {
        if (movement == null) return false;

        // Check if grounded and moving
        bool isGrounded = movement.IsGrounded;
        bool isMoving = movement.MovementVelocity.sqrMagnitude > 0.1f;

        return isGrounded && isMoving;
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip);
    }
}
