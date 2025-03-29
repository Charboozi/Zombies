using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private TextMeshProUGUI interactTextUI;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;

    private Interactable currentInteractable;

    private void Update()
    {
        HandleRaycast();

        if (currentInteractable != null)
        {
            interactTextUI.text = currentInteractable.GetInteractText();

            if (Input.GetKeyDown(interactKey) && !currentInteractable.isCoolingDown.Value)
            {
                currentInteractable.Interact();
            }
        }
        else
        {
            interactTextUI.text = "";
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
