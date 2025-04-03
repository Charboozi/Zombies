using UnityEngine;
using Unity.Netcode;

public class ChargeStation : NetworkBehaviour, IInteractableAction, IBroadcastClientAction
{
    [SerializeField] private ParticleSystem chargeEffect;

    public void DoAction()
    {
        if (!IsServer) return;

        foreach (var chargeComponent in FindObjectsByType<InteractableCharge>(FindObjectsSortMode.None))
        {
            chargeComponent.FullyRecharge();
        }

        Debug.Log("🔋 ChargeStation recharged all interactables (synced to everyone).");
    }
    public void DoAllClientsAction()
    {
        if (chargeEffect != null)
        chargeEffect.Play();
        FlashEffect.Instance.TriggerFlash();
    }
}
