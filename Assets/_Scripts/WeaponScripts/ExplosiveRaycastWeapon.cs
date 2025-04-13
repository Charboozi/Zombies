using UnityEngine;

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

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {

            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "ExplosionImpact");
            }
            
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

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }
}
