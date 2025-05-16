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

            if (hit.collider.TryGetComponent(out LimbHealth limb))
            {
                float limbDamage = damage;
                if (limb.limbID.ToLower().Contains("head"))
                    limbDamage *= headshotMultiplier;

                limb.TakeLimbDamageServerRpc(Mathf.RoundToInt(limbDamage));

                if (NetworkImpactSpawner.Instance != null)
                {
                    string fx = limb.limbID.ToLower().Contains("head") ? "BloodImpactHeadshot" : "BloodImpact";
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, fx);
                }

                if (!canPierceEnemies)
                    break;

                continue;
            }

            if (hit.collider.TryGetComponent(out EntityHealth bodyEntity))
            {
                bool isPlayer = bodyEntity.CompareTag("Player");

                // 🛡️ Do NOT apply damage to players if PvP is ON
                if (isPlayer && GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode)
                {
                    Debug.Log("⚠️ PvP is ON — skipping player damage.");
                }
                else
                {
                    bodyEntity.TakeDamageServerRpc(Mathf.RoundToInt(damage));

                    if (NetworkImpactSpawner.Instance != null)
                        NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }

                if (!canPierceEnemies)
                    break;

                continue;
            }

            if (NetworkImpactSpawner.Instance != null)
            {
                var balloon = hit.collider.GetComponent<Balloon>();
                if (balloon != null)
                {
                    Debug.Log("🎯 Hit a Balloon: " + balloon.name);
                    balloon.TryPop(); // ✅ pops it
                }
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
