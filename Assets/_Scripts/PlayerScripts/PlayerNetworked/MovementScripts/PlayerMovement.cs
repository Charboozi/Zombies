using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class NetworkedCharacterMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 2.0f;
    public float gravity = 9.8f;
    public float groundCheckRadius = 0.3f;
    public float lerpRate = 10f; // Interpolation for remote players

    [Header("Ground Check Settings")]
    public LayerMask groundLayer; // Assign in Inspector to detect ground

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    private float originalMoveSpeed;
    private Coroutine slowCoroutine;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (IsOwner)
        {
            velocity = Vector3.zero; // Reset velocity
        }

        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (IsOwner)
        {
            CheckGrounded(); // **Better ground detection**
            ProcessMovement();
            ProcessJump();
        }
    }

    private void ProcessMovement()
    {
        // Get input
        float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float moveZ = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        // Move relative to player rotation
        Vector3 moveDir = (transform.forward * moveZ + transform.right * moveX).normalized;

        if (moveDir.sqrMagnitude > 0.01f) // Prevent small unnecessary movements
        {
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }

    private void ProcessJump()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        if (isGrounded)
        {
            if (jumpPressed)
            {
                velocity.y = Mathf.Sqrt(jumpForce * 2f * gravity);
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime; // Apply gravity
        }

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
        moveSpeed = originalMoveSpeed * (1f - slowFactor);
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }
}
