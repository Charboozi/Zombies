using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 6; // Number of pellets fired per shot
    public float spreadAngle = 10f; // Spread angle in degrees

    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot()) return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Debug.Log("Shotgun fired!");

        for (int i = 0; i < pelletsPerShot; i++)
        {
            FirePellet();
        }

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    private void FirePellet()
    {
        Ray ray = new Ray(playerCamera.transform.position, GetSpreadDirection());
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            float finalDamage = damage / pelletsPerShot;

            // ✅ Headshot check first
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

                    continue; // No piercing for shotgun, continue to next pellet
                }
            }

            // ✅ Body hit
            if (hit.collider.TryGetComponent(out EntityHealth bodyEntity))
            {
                bodyEntity.TakeDamageServerRpc(Mathf.RoundToInt(finalDamage));

                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }

                break; // Stop at first valid hit
            }

            // ✅ Environment hit
            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
            }

            break;
        }
    }

    private Vector3 GetSpreadDirection()
    {
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
        return spreadRotation * playerCamera.transform.forward;
    }
}
