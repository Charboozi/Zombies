using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot())
            return;

        // Deduct ammo and perform a raycast to simulate shooting
        currentAmmo--;

        UpdateEmissionIntensity();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Pistol hit: " + hit.collider.name);

            // Apply damage to the hit entity
            EntityHealth entity = hit.collider.GetComponent<EntityHealth>();
            if (entity != null)
            {
                entity.TakeDamageServerRpc(damage);
            }

            // Spawn an impact effect via the network spawner
            if (networkImpactSpawner != null)
            {
                networkImpactSpawner.SpawnImpactEffectServerRpc(hit.point, hit.normal, impactEffectPrefab.name);
            }
        }

        // Play muzzle flash effect
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
}
