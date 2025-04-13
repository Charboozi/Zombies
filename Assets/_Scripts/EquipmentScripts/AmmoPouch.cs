using UnityEngine;

public class AmmoPouch : BaseEquipment
{
    [SerializeField] private int multiplier = 2; // Default: double the ammo

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplyAmmoBoost();
    }

    private void OnDisable()
    {
        RemoveAmmoBoost();
    }

    private void ApplyAmmoBoost()
    {
        if (effectApplied) return;

        AmmoMultiplierManager.Instance.SetMultiplier(multiplier);
        effectApplied = true;
        Debug.Log($"{gameObject.name} applied ammo multiplier boost: x{multiplier}");
    }

    private void RemoveAmmoBoost()
    {
        if (!effectApplied) return;

        AmmoMultiplierManager.Instance.ResetMultiplier();
        Debug.Log($"{gameObject.name} removed ammo multiplier boost.");
        effectApplied = false;
    }
}
