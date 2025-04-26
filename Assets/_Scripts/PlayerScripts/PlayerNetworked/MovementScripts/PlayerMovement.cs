using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class NetworkedCharacterMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f;
    public float jumpForce = 2.0f;
    public float gravity = 9.8f;
    public float groundCheckRadius = 0.3f;
    public float lerpRate = 10f;

    [Header("Ground Check Settings")]
    public LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    private float bonusSpeed = 0f;
    private float slowMultiplier = 1f;
    private Coroutine slowCoroutine;

    private Transform currentLift;
    private Vector3 lastLiftPosition;

    public Vector3 MovementVelocity { get; private set; }

    private EntityHealth entityHealth;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        entityHealth = GetComponent<EntityHealth>();

        if (IsOwner)
        {
            velocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (currentLift != null)
        {
            Vector3 liftDelta = currentLift.position - lastLiftPosition;
            controller.Move(liftDelta);
            lastLiftPosition = currentLift.position;
        }

        if (entityHealth != null && entityHealth.isDowned.Value)
        {
            return; // Player is downed, disable movement
        }

        CheckGrounded();
        ProcessMovement();
        ProcessJump();
    }

    private void ProcessMovement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.forward * moveZ + transform.right * moveX).normalized;

        MovementVelocity = moveDir * CurrentMoveSpeed;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            controller.Move(moveDir * CurrentMoveSpeed * Time.deltaTime);
        }
    }

    private void ProcessJump()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        if (isGrounded && velocity.y < 0f)
        {
            // 👇 Reset downward velocity when grounded
            velocity.y = -2f; // Small downward force to keep grounded
        }

        if (isGrounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpForce * 2f * gravity);
        }

        // Apply gravity every frame
        velocity.y -= gravity * Time.deltaTime;

        // Move with vertical velocity
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckGrounded()
    {
        Vector3 spherePosition = transform.position + Vector3.down * (controller.height / 2f);
        isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundLayer);
    }

    public void ApplyTemporarySlow(float slowFactor, float duration)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(SlowDownCoroutine(slowFactor, duration));
    }

    private IEnumerator SlowDownCoroutine(float slowFactor, float duration)
    {
        slowMultiplier = 1f - slowFactor;
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1f;
    }

    public void AddBonusSpeed(float amount)
    {
        bonusSpeed += amount;
    }

    public void RemoveBonusSpeed(float amount)
    {
        bonusSpeed -= amount;
        if (bonusSpeed < 0f) bonusSpeed = 0f;
    }

    public float CurrentMoveSpeed => (baseMoveSpeed + bonusSpeed) * slowMultiplier;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Lift"))
        {
            currentLift = other.transform;
            lastLiftPosition = currentLift.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Lift"))
        {
            currentLift = null;
        }
    }
}
