using UnityEngine;
using Unity.Netcode;

public class PickupAmmo : MonoBehaviour
{
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask ammoLayer;

    private void OnEnable() => PlayerInput.OnInteractPressed += AttemptPickup;
    private void OnDisable() => PlayerInput.OnInteractPressed -= AttemptPickup;

    private void AttemptPickup()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, pickupRange, ammoLayer))
        {
            var pickup = hit.collider.GetComponent<NetworkedPickupableItem>();
            if (pickup == null) return;

            // ✅ Give ammo to local weapon
            GiveAmmoToLocalWeapon();

            // ✅ Tell the server to despawn the pickup object
            pickup.Despawn();
        }
    }

    private void GiveAmmoToLocalWeapon()
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance.CurrentWeapon;
        if (weapon != null)
        {
            int amountToGive = weapon.maxAmmo * 3 * AmmoMultiplierManager.Instance.AmmoMultiplier; // You can also use ammoAmount if you want custom value
            weapon.reserveAmmo += amountToGive;
            Debug.Log($"[Client {NetworkManager.Singleton.LocalClientId}] Ammo pickup: +{amountToGive} reserve ammo.");
        }
        else
        {
            Debug.LogWarning("PickupAmmo: No current weapon found on client.");
        }
    }
}
