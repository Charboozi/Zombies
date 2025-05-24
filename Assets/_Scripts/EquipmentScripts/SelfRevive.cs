using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SelfRevive : BaseEquipment
{
    private EntityHealth entityHealth;
    private bool used = false;

    [SerializeField] private float selfReviveDelay = 3f; // Match revive system timing

    private void OnEnable()
    {
        used = false;

        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer == null) return;

        var newHealth = localPlayer.GetComponent<EntityHealth>();
        if (entityHealth != null && entityHealth != newHealth)
        {
            entityHealth.OnDowned -= HandleDowned; // Remove old link
        }

        entityHealth = newHealth;

        if (entityHealth != null)
        {
            entityHealth.OnDowned -= HandleDowned; // Prevent double registration
            entityHealth.OnDowned += HandleDowned;
        }
    }

    private void OnDisable()
    {
        used = false;

        if (entityHealth != null)
        {
            entityHealth.OnDowned -= HandleDowned;
            entityHealth = null;
        }
    }

    private void HandleDowned(EntityHealth downedTarget)
    {
        if (used || entityHealth == null || downedTarget != entityHealth) return;

        used = true;
        Debug.Log("⏳ SelfRevive will trigger after delay...");

        // Start the timed revive sequence
        StartCoroutine(DelayedSelfRevive());
    }

    private IEnumerator DelayedSelfRevive()
    {
        float elapsed = 0f;
        while (elapsed < selfReviveDelay)
        {
            if (!entityHealth.isDowned.Value)
            {
                Debug.Log("🛑 SelfRevive cancelled (player already revived).");
                used = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        RequestReviveSelfServerRpc();

        // Soft remove after revive
        EquipmentInventory inventory = GetComponentInParent<EquipmentInventory>();
        if (inventory != null)
        {
            inventory.Unequip(gameObject.name);
        }

        Debug.Log("✅ SelfRevive triggered after delay and unequipped.");
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestReviveSelfServerRpc()
    {
        if (entityHealth != null && entityHealth.isDowned.Value)
        {
            entityHealth.Revive(); // 10 HP + revive
        }
    }
}
