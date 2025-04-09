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

        currentAmmo--;
        UpdateEmissionIntensity();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Apply damage to the hit entity
            EntityHealth entity = hit.collider.GetComponent<EntityHealth>();
            if (entity != null)
            {
                entity.TakeDamageServerRpc(damage);

                // Blood effect on entity hit
                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }
            }
            else
            {
                // Regular impact effect
                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
                }
            }
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

}
