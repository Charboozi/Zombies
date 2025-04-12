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
    RaycastHit[] hits = Physics.RaycastAll(ray, range);

    // ✅ Sort hits by distance!
    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

    foreach (var hit in hits)
    {
        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (hit.collider.TryGetComponent(out EntityHealth entity))
            {
                entity.TakeDamageServerRpc(damage);

                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }
            }

            if (canPierceEnemies)
                continue;
            else
                break;
        }
        else
        {
            // Hit non-enemy object (wall, etc.)
            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
            }

            break;
        }
    }


        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

}
