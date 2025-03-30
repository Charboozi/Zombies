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
}
