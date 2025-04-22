using UnityEngine;
using Unity.Netcode;

public class ExplosiveRaycastWeapon : WeaponBase
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 5f;

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

        // Tell the server to process the shot
        ShootServerRpc(playerCamera.transform.position, playerCamera.transform.forward);

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 origin, Vector3 direction)
    {
        Ray ray = new Ray(origin, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Spawn explosion effect on hit point
            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "ExplosionImpact");
            }

            // Apply AoE damage to nearby entities
            Collider[] colliders = Physics.OverlapSphere(hit.point, explosionRadius);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out EntityHealth entity))
                {
                    float distance = Vector3.Distance(hit.point, collider.transform.position);
                    float multiplier = Mathf.Clamp01(1f - (distance / explosionRadius));
                    int damageToApply = Mathf.RoundToInt(damage * multiplier);

                    entity.TakeDamageServerRpc(damageToApply);
                }
            }
        }
    }
}
