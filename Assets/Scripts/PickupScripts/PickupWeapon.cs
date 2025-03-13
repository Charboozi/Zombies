using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PickupWeapon : NetworkBehaviour
{
    public float pickupRange = 3f;
    public LayerMask weaponLayer;
    public KeyCode pickupKey = KeyCode.E;
    public WeaponSwitcher weaponSwitcher;

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey))
            AttemptPickup();
    }

    void AttemptPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange, weaponLayer))
        {
            // Look for the pickup script on the hit object.
            WeaponPickupNetworked pickupScript = hit.collider.GetComponent<WeaponPickupNetworked>();
            if (pickupScript != null)
            {
                // Use the pickup's GameObject name as the weapon key.
                string weaponName = pickupScript.gameObject.name;

                // Check if the weapon is already active.
                if (weaponSwitcher != null && weaponSwitcher.activeWeapons.Any(w => w.name == weaponName))
                {
                    Debug.Log($"Weapon '{weaponName}' is already active. Not picking it up.");
                    return;
                }

                // Check if the inventory is full.
                if (weaponSwitcher.activeWeapons.Count >= weaponSwitcher.maxWeapons)
                {
                    Debug.Log($"⚠️ Inventory full! Cannot pick up '{weaponName}'.");
                    return;
                }

                // Add the weapon (by name) to the inventory.
                weaponSwitcher.PickUpWeapon(weaponName);

                // Despawn the pickup object over the network.
                pickupScript.DespawnWeapon();
            }
            else
            {
                Debug.LogWarning("No WeaponPickupNetworked script found on this object!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * pickupRange);
    }
}
