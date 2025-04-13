using UnityEngine;
using Unity.Netcode;

public class RegenerationBooster : BaseEquipment
{
    [SerializeField] private float regenSpeedMultiplier = 0.5f; // Example: 0.5f makes regen twice as fast

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplyRegenBoost();
    }

    private void OnDisable()
    {
        RemoveRegenBoost();
    }

    private void ApplyRegenBoost()
    {
        if (effectApplied) return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
            if (healthProxy != null)
            {
                healthProxy.MultiplyRegenInterval(regenSpeedMultiplier);
                effectApplied = true;
                Debug.Log($"{gameObject.name} applied regen speed boost: x{regenSpeedMultiplier}");
            }
        }
    }

    private void RemoveRegenBoost()
    {
        if (!effectApplied) return;

        // 🧩 Check if shutting down
        if (NetworkManager.Singleton == null || NetworkManager.Singleton.ShutdownInProgress || NetworkManager.Singleton.LocalClient == null)
        {
            effectApplied = false;
            return;
        }

        var localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayerObj != null)
        {
            var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
            if (healthProxy != null)
            {
                healthProxy.MultiplyRegenInterval(1f / regenSpeedMultiplier); // Restore to original
                Debug.Log($"{gameObject.name} removed regen speed boost: x{regenSpeedMultiplier}");
            }
        }

        effectApplied = false;
    }

}
