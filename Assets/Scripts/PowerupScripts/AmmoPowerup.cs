using UnityEngine;
using Unity.Netcode;

public class AmmoPowerup : PowerupBase
{
    [Tooltip("The amount of reserve ammo to add.")]
    public int ammoAmount = 30;

    // Return the ammo amount as the effect value.
    protected override int GetEffectValue() => ammoAmount;

    // On the client, find the local WeaponStats and add the ammo.
    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int effectValue, ClientRpcParams clientRpcParams = default)
    {
        WeaponBase weapon = WeaponManager.Instance.CurrentWeapon;
        if (weapon != null)
        {
            weapon.reserveAmmo += effectValue;
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Ammo powerup applied: +{effectValue} reserve ammo.");
        }
        else
        {
            Debug.LogWarning("WeaponStats not found on this client.");
        }
    }
}
