using UnityEngine;
using System;

/// <summary>
/// Add to player to be able to interact with interactables
/// </summary>
public class Interactor : MonoBehaviour
{
    public event Action<string> OnInteractTextChanged;

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    private Interactable currentInteractable;

    private void Update()
    {
        HandleRaycast();

        if (currentInteractable != null)
        {
            OnInteractTextChanged?.Invoke(currentInteractable.GetInteractText());

            if (Input.GetKeyDown(interactKey) && !currentInteractable.isCoolingDown.Value)
            {
                currentInteractable.Interact();
            }
        }
        else
        {
            OnInteractTextChanged?.Invoke(string.Empty);
        }
    }

    private void HandleRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            if (hit.collider.TryGetComponent(out Interactable interactable))
            {
                if (currentInteractable != interactable)
                    currentInteractable = interactable;

                return;
            }
        }

        // If we get here, nothing valid was hit
        currentInteractable = null;
    }
}
