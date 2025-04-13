using UnityEngine;
using Unity.Netcode;

public class PickupEquipment : MonoBehaviour
{
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask equipmentLayer;
    [SerializeField] private EquipmentInventory equipmentInventory; // Your local inventory system

    private void OnEnable() => PlayerInput.OnInteractPressed += AttemptPickup;
    private void OnDisable() => PlayerInput.OnInteractPressed -= AttemptPickup;

    private void AttemptPickup()
    {
        if (!NetworkManager.Singleton.IsClient) return;
        
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, pickupRange, equipmentLayer))
        {
            var pickup = hit.collider.GetComponent<NetworkedPickupableItem>();
            if (pickup == null) return;
            
            string equipmentName = pickup.gameObject.name;

            // Check if this equipment is already equipped.
            if (equipmentInventory.HasEquipped(equipmentName))
            {
                Debug.Log($"Equipment '{equipmentName}' is already active. Not picking it up.");
                return;
            }

            // Equip it in the local inventory (your own logic).
            equipmentInventory.Equip(equipmentName);

            // Now, tell the server to update the player's model visuals.
            // Look up the local player's network object.
            var localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayerObj != null)
            {
                var proxy = localPlayerObj.GetComponent<EquipmentVisualProxy>();
                if (proxy != null)
                {
                    proxy.RequestEquipEquipmentServerRpc(equipmentName);
                }
            }

            // Despawn the pickup object over the network.
            pickup.Despawn();
        }
    }
}
