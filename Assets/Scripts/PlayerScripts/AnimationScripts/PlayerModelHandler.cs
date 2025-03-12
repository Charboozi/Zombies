using UnityEngine;
using Unity.Netcode;

public class PlayerModelHandler : NetworkBehaviour
{
    [Header("References")]
    public GameObject playerModel; // Assign the full-body model
    public Animator animator; // Assign the Animator component

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (IsOwner)
        {
            HideLocalPlayerModel();
        }
    }

    void Update()
    {
        if (!IsOwner || animator == null || characterController == null) return;

        // Check if player is grounded
        bool isGrounded = characterController.isGrounded;

        // Check if player is pressing movement keys
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        // Play walking animation ONLY if grounded and moving
        animator.SetBool("isWalking", isGrounded && isMoving);
    }

    private void HideLocalPlayerModel()
    {
        if (playerModel == null) return;

        // Get all SkinnedMeshRenderers and MeshRenderers in the player model
        SkinnedMeshRenderer[] skinnedMeshes = playerModel.GetComponentsInChildren<SkinnedMeshRenderer>();
        MeshRenderer[] meshRenderers = playerModel.GetComponentsInChildren<MeshRenderer>();

        foreach (var mesh in skinnedMeshes)
        {
            mesh.enabled = false; // Hides skinned meshes
        }

        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = false; // Hides regular meshes
        }

    }

}
