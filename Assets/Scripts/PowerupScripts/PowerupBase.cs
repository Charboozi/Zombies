using UnityEngine;
using Unity.Netcode;

public abstract class PowerupBase : NetworkBehaviour
{
    [Tooltip("If true, all players get the effect; otherwise, only the player who triggers it.")]
    public bool applyToAll = false;

    // Trigger detection common to all powerups.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Retrieve the player's network identity.
        NetworkObject playerNetworkObject = other.GetComponent<NetworkObject>();
        if (playerNetworkObject == null)
            return;

        // Request the powerup effect on the server.
        CollectPowerupServerRpc(playerNetworkObject.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CollectPowerupServerRpc(ulong triggeringClientId)
    {
        if (applyToAll)
        {
            // Apply the effect to all players.
            ApplyPowerupClientRpc(GetEffectValue(), new ClientRpcParams());
        }
        else
        {
            // Apply the effect only to the triggering client.
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { triggeringClientId } }
            };
            ApplyPowerupClientRpc(GetEffectValue(), clientRpcParams);
        }

        // Despawn or destroy the powerup so it’s removed from the scene.
        GetComponent<NetworkObject>().Despawn(true);
    }

    // Derived classes override this to specify the effect's magnitude (e.g., ammo amount, health value, etc.)
    protected abstract int GetEffectValue();

    // Derived classes override this ClientRpc to implement the specific effect.
    [ClientRpc]
    protected virtual void ApplyPowerupClientRpc(int effectValue, ClientRpcParams clientRpcParams = default)
    {
        Debug.LogWarning("ApplyPowerupClientRpc was called but not implemented in derived class.");
    }
}
