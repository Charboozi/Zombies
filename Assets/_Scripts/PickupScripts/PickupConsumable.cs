using UnityEngine;
using Unity.Netcode;

public class PickupConsumable : MonoBehaviour
{
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask consumableLayer;

    private void OnEnable() => PlayerInput.OnInteractPressed += AttemptPickup;
    private void OnDisable() => PlayerInput.OnInteractPressed -= AttemptPickup;

    private void AttemptPickup()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, pickupRange, consumableLayer))
        {
            var pickup = hit.collider.GetComponent<ConsumablePickupItem>();
            if (pickup == null) return;

            // ✅ Add to consumable manager
            if (ConsumableManager.Instance != null)
            {
                ConsumableManager.Instance.Add(pickup.ConsumableType, pickup.Amount);
                Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Picked up {pickup.Amount}x {pickup.ConsumableType}");
            }

            // ✅ Request server to despawn
            pickup.Despawn();
        }
    }
}
