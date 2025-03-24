using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class PickupWeapon : NetworkBehaviour
{
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask weaponLayer;

    [SerializeField] private WeaponInventory weaponInventory;

    void Awake()
    {
        if (weaponInventory == null)
        Debug.LogError("WeaponInventory missing!");
    }

    private void OnEnable()
    {
        PlayerInput.OnInteractPressed += AttemptPickup;
    }
    private void OnDisable()
    {
        PlayerInput.OnInteractPressed -= AttemptPickup;
    }

    void AttemptPickup()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, pickupRange, weaponLayer))
        {
            WeaponPickupNetworked pickupScript = hit.collider.GetComponent<WeaponPickupNetworked>();
            if (pickupScript != null)
            {
                string weaponName = pickupScript.gameObject.name;

                if (weaponInventory != null && weaponInventory.Weapons.Any(w => w.name == weaponName))
                {
                    Debug.Log($"Weapon '{weaponName}' is already active. Not picking it up.");
                    return;
                }

                // Add the weapon (by name) to the inventory.
                weaponInventory.PickUpWeapon(weaponName);

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
