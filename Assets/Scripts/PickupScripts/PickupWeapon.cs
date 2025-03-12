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
        {
            AttemptPickup();
        }
    }

    void AttemptPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange, weaponLayer))
        {
            WeaponPickupNetworked weaponScript = hit.collider.GetComponent<WeaponPickupNetworked>();
            if (weaponScript != null)
            {
                string weaponName = weaponScript.gameObject.name;

                // Check if weapon is already active
                if (weaponSwitcher != null && weaponSwitcher.activeWeapons.Any(w => w.name == weaponName))
                {
                    Debug.Log($"Weapon '{weaponName}' is already active. Not picking it up.");
                    return; 
                }

                // Check if inventory is full
                if (weaponSwitcher.activeWeapons.Count >= weaponSwitcher.maxWeapons) // Assuming maxWeapons is 3
                {
                    Debug.Log($"⚠️ Inventory full! Cannot pick up '{weaponName}'.");
                    return;
                }
                    weaponSwitcher.PickUpWeapon(weaponScript.gameObject.name);
                    // Tell the weapon to despawn itself
                    weaponScript.DespawnWeapon();
            }
            else
            {
                Debug.LogWarning("No WeaponNetworked script found on this object!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * pickupRange);
    }
}
