using UnityEngine;
using Unity.Netcode;

public class WeaponPickupNetworked : NetworkBehaviour
{
    public void DespawnWeapon()
    {
        if (IsServer)
        {
            Debug.Log($"✅ Server despawning weapon: {gameObject.name}");
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            RequestDespawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc()
    {
        Debug.Log($"🔹 ServerRpc received to despawn weapon: {gameObject.name}");
        GetComponent<NetworkObject>().Despawn();
    }
}
