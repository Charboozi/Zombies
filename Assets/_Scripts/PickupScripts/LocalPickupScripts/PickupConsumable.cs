using UnityEngine;
using Unity.Netcode;

public class PickupConsumable : BaseLocalPickup
{
    protected override bool OnPickupFound(NetworkedPickupableItem pickup)
    {
        if (ConsumableManager.Instance != null)
        {
            ConsumableManager.Instance.Add(pickup.name, 1);
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Picked up 1x {pickup.name}");

            DespawnPickup(pickup);
            return true; // ✅ Successful pickup, play sound
        }

        Debug.LogWarning("ConsumableManager not found, cannot pick up item.");
        return false; // ❌ Pickup failed
    }
}
