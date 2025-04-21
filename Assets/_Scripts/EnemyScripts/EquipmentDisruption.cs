using System.Linq;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(EnemyAttack))]
public class EquipmentDisruption : NetworkBehaviour
{
    [Header("Equipment Disruption")]
    [Tooltip("If true, this enemy will randomly disable one of the local player's equipped items on hit.")]
    public bool canRemoveEquipment = true;

    private EnemyAttack attackModule;

    private void Awake()
    {
        attackModule = GetComponent<EnemyAttack>();
    }

    private void OnEnable()
    {
        attackModule.OnTargetHit += HandleTargetHit;
    }

    private void OnDisable()
    {
        attackModule.OnTargetHit -= HandleTargetHit;
    }

    private void HandleTargetHit(Transform hitTarget)
    {
        if (!IsServer || !canRemoveEquipment)
            return;

        // Grab the hit player's NetworkObject
        if (hitTarget.TryGetComponent(out NetworkObject netObj))
        {
            var targetClientId = netObj.OwnerClientId;

            // Send a ClientRpc to exactly that client
            RemoveRandomEquipmentClientRpc(new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[]{ targetClientId }
                }
            });
        }
    }

    // This runs only on the target player's client
    [ClientRpc]
    private void RemoveRandomEquipmentClientRpc(ClientRpcParams rpcParams = default)
    {
        // Find _your_ local inventory component
        var localInventory = FindFirstObjectByType<EquipmentInventory>();
        if (localInventory == null) return;

        var equippedItems = localInventory.EquippedItems.ToList();
        if (equippedItems.Count == 0) return;

        // Pick one at random and disable it
        int idx = Random.Range(0, equippedItems.Count);
        var item = equippedItems[idx];
        item.SetActive(false);

        // Hide its UI icon
        if (item.TryGetComponent(out BaseEquipment baseEquip) &&
            localInventory.uiManager != null)
        {
            localInventory.uiManager.HideIcon(baseEquip.equipmentIcon);
        }

        Debug.Log($"❌ Equipment removed locally: {item.name}");
    }
}
