using UnityEngine;
using Unity.Netcode;

public class ConsumablePickupItem : NetworkBehaviour
{
    public string ConsumableType = "Keycard"; // Set in inspector
    public int Amount = 1;

    public void Despawn()
    {
        if (IsServer)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Debug.LogWarning("Only the server can despawn this item.");
        }
    }
}
