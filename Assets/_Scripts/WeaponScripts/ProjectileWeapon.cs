using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [Header("Projectile Settings")]
    [SerializeField] private float baseSpreadAngle = 0f;

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

        // ✅ Start with straight direction
        Vector3 direction = playerCamera.transform.forward;

        // ✅ Apply spread if any
        if (baseSpreadAngle > 0f)
        {
            direction = ApplySpread(direction);
        }

        Ray ray = new Ray(playerCamera.transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            float finalDamage = damage;

            if (hit.collider.CompareTag("Headshot"))
            {
                if (hit.collider.transform.parent.TryGetComponent(out EntityHealth headEntity))
                {
                    finalDamage *= headshotMultiplier;
                    headEntity.TakeDamageServerRpc(Mathf.RoundToInt(finalDamage));

                    if (NetworkImpactSpawner.Instance != null)
                    {
                        NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                    }

                    if (!canPierceEnemies)
                        break;

                    continue;
                }
            }

            if (hit.collider.TryGetComponent(out EntityHealth bodyEntity))
            {
                bodyEntity.TakeDamageServerRpc(Mathf.RoundToInt(finalDamage));

                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }

                if (!canPierceEnemies)
                    break;

                continue;
            }

            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
            }

            break;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        if (baseSpreadAngle <= 0f)
            return direction;

        float spreadRadius = Mathf.Tan(baseSpreadAngle * Mathf.Deg2Rad);
        Vector2 randomPoint = Random.insideUnitCircle * spreadRadius;

        Vector3 spreadDirection = direction;
        spreadDirection += playerCamera.transform.right * randomPoint.x;
        spreadDirection += playerCamera.transform.up * randomPoint.y;

        return spreadDirection.normalized;
    }

}
