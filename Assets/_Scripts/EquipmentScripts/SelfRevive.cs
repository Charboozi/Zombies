using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SelfRevive : NetworkBehaviour
{
    [SerializeField] private float selfReviveDelay = 3f; 

    private EntityHealth entityHealth;
    private bool used = false;

    public override void OnNetworkSpawn()
    {
        // Only hook up the local player
        if (!IsOwner) return;

        // Grab our own EntityHealth
        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        entityHealth = playerObj.GetComponent<EntityHealth>();
        if (entityHealth == null) return;

        // Subscribe to the networked downed flag
        entityHealth.isDowned.OnValueChanged += OnDownedChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (entityHealth != null)
            entityHealth.isDowned.OnValueChanged -= OnDownedChanged;
    }

    private void OnDownedChanged(bool previous, bool current)
    {
        // When we just went down, trigger once
        if (current && !used)
        {
            used = true;
            StartCoroutine(DelayedSelfRevive());
        }
    }

    private IEnumerator DelayedSelfRevive()
    {
        float t = 0f;
        while (t < selfReviveDelay)
        {
            // If revived by someone else first, cancel
            if (!entityHealth.isDowned.Value)
            {
                used = false;
                yield break;
            }
            t += Time.deltaTime;
            yield return null;
        }

        // Ask server to revive us
        RequestReviveSelfServerRpc();

        // Remove this equipment locally
        var inv = GetComponentInParent<EquipmentInventory>();
        inv?.Unequip(gameObject.name);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestReviveSelfServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server‐side check & revive
        if (entityHealth != null && entityHealth.isDowned.Value)
            entityHealth.Revive();
    }
}
