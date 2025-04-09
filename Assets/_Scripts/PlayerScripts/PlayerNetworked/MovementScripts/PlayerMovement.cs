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
    public float lerpRate = 10f;

    [Header("Ground Check Settings")]
    public LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    private float originalMoveSpeed;
    private Coroutine slowCoroutine;

    private EntityHealth entityHealth;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        entityHealth = GetComponent<EntityHealth>();

        if (IsOwner)
        {
            velocity = Vector3.zero;
        }

        originalMoveSpeed = moveSpeed;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (entityHealth != null && entityHealth.isDowned.Value)
        {
            // Player is downed, disable movement.
            return;
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

        if (moveDir.sqrMagnitude > 0.01f)
        {
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }

    private void ProcessJump()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        if (isGrounded && jumpPressed)
        {
            velocity.y = Mathf.Sqrt(jumpForce * 2f * gravity);
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
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
