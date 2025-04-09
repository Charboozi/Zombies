using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class InteractableChargeManager : MonoBehaviour
{
    public static InteractableChargeManager Instance { get; private set; }

    [SerializeField] private LayerMask interactableLayer;

    private readonly List<InteractableCharge> charges = new List<InteractableCharge>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        FindAllInteractables();
    }

    private void FindAllInteractables()
    {
        charges.Clear();

        // Find all colliders in the scene on the interactable layer
        var colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);

        foreach (var collider in colliders)
        {
            if (((1 << collider.gameObject.layer) & interactableLayer.value) != 0)
            {
                var charge = collider.GetComponent<InteractableCharge>();
                if (charge != null)
                {
                    charges.Add(charge);
                }
            }
        }

        Debug.Log($"✅ Found {charges.Count} interactable charges in the scene.");
    }

    public void FullyRechargeAll()
    {
        if (!IsServer()) return;

        foreach (var charge in charges)
        {
            charge.FullyRecharge();
        }

        Debug.Log("🔋 Fully recharged all interactables.");
    }

    public void FullyDischargeAll()
    {
        if (!IsServer()) return;

        foreach (var charge in charges)
        {
            charge.FullyDischarge();
        }

        Debug.Log("⚡ Fully discharged all interactables.");
    }

    private bool IsServer()
    {
#if UNITY_SERVER || UNITY_EDITOR
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
#else
        return false;
#endif
    }
}
