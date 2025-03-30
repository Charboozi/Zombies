using UnityEngine;
using Unity.Netcode;

public class ChargeStation : NetworkBehaviour, IInteractableAction
{
    public void DoAction()
    {
        if (!IsServer) return;

        foreach (var chargeComponent in FindObjectsByType<InteractableCharge>(FindObjectsSortMode.None))
        {
            chargeComponent.FullyRecharge();
        }

        Debug.Log("🔋 ChargeStation recharged all interactables (synced to everyone).");
    }
}
