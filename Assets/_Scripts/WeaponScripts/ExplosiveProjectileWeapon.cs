using UnityEngine;
using Unity.Netcode;

public class ExplosiveWeapon : WeaponBase
{
    [Header("Explosive Weapon Settings")]
    [Tooltip("The explosive projectile prefab. This should have an ExplosiveProjectile script attached.")]
    public GameObject explosiveProjectilePrefab;
    
    [Tooltip("The force to apply to the projectile when fired.")]
    public float projectileForce = 1000f;

    protected override void Start()
    {
        base.Start();
        // Make sure any weapon-specific values are set in the Inspector.
        // For example: maxAmmo, currentAmmo, reserveAmmo, damage, range, fireRate, etc.
    }

    public override void Shoot()
    {
        if (!CanShoot())
            return;

        // Deduct ammo.
        currentAmmo--;

        UpdateEmissionIntensity();

        // Instantiate the projectile at the muzzle position and rotation.
        if (explosiveProjectilePrefab != null && muzzleTransform != null)
        {
            GameObject projectileObj = Instantiate(explosiveProjectilePrefab, muzzleTransform.position, muzzleTransform.rotation);

            // Apply force to the projectile so it is fired.
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(muzzleTransform.forward * projectileForce);
            }
        }
        else
        {
            Debug.LogWarning("Explosive projectile prefab or muzzle transform not assigned!");
        }

        // Play muzzle flash effect if available.
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
}
