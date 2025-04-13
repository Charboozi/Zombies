using UnityEngine;
using Unity.Netcode;

public class HealthProxy : NetworkBehaviour
{
    private EntityHealth entityHealth;

    private void Awake()
    {
        entityHealth = GetComponent<EntityHealth>();
        if (entityHealth == null)
            Debug.LogError("EntityHealth component not found on player object.");
    }

    // These public methods are called locally by equipment items.
    public void AddArmor(int bonus)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestAddArmorServerRpc(bonus);
    }

    public void RemoveArmor(int bonus)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestRemoveArmorServerRpc(bonus);
    }

    public void FullHeal()
    {
        if (NetworkManager.Singleton.IsClient)
            RequestFullHealServerRpc();
    }

    public void MultiplyRegenInterval(float multiplier)
    {
        if (NetworkManager.Singleton.IsClient)
            RequestMultiplyRegenIntervalServerRpc(multiplier);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestAddArmorServerRpc(int bonus, ServerRpcParams rpcParams = default)
    {
        entityHealth.AddArmor(bonus);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestRemoveArmorServerRpc(int bonus, ServerRpcParams rpcParams = default)
    {
        entityHealth.RemoveArmor(bonus);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestFullHealServerRpc(ServerRpcParams rpcParams = default)
    {
        entityHealth.FullHeal();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestMultiplyRegenIntervalServerRpc(float multiplier, ServerRpcParams rpcParams = default)
    {
        entityHealth.regenInterval *= multiplier;
        Debug.Log($"{gameObject.name}: Regen interval modified by multiplier {multiplier}. New regen interval: {entityHealth.regenInterval}");
    }
}
