using UnityEngine;
using Unity.Netcode;

public class AutoReviveInjector : BaseEquipment
{
    private EntityHealth entityHealth;
    private bool used = false;

    private void OnEnable()
    {
        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer == null) return;

        entityHealth = localPlayer.GetComponent<EntityHealth>();
        if (entityHealth != null)
        {
            entityHealth.OnDowned += HandleDowned;
        }
    }

    private void OnDisable()
    {
        if (entityHealth != null)
        {
            entityHealth.OnDowned -= HandleDowned;
        }
    }

    private void HandleDowned(EntityHealth downedTarget)
    {
        if (used || entityHealth == null || downedTarget != entityHealth) return;

        used = true;

        // ✅ Send revive command to server
        RequestReviveSelfServerRpc();

        // ✅ Soft remove: unequip instead of destroy
        EquipmentInventory inventory = GetComponentInParent<EquipmentInventory>();
        if (inventory != null)
        {
            inventory.Unequip(gameObject.name); // Deactivates and hides icon
        }
        else
        {
            gameObject.SetActive(false); // Fallback
        }

        Debug.Log($"⚡ AutoReviveInjector triggered and unequipped.");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestReviveSelfServerRpc()
    {
        if (entityHealth != null && entityHealth.isDowned.Value)
        {
            entityHealth.Revive(); // Heals 10 HP and sets isDowned = false
        }
    }
}
