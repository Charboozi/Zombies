using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 6; // Number of pellets fired per shot
    public float spreadAngle = 10f; // Spread angle in degrees

    protected override void Start()
    {
        base.Start(); // Ensure base class initialization
    }

    public override void Shoot()
    {
        if (!CanShoot()) return;

        currentAmmo--; // Deduct ammo

        UpdateEmissionIntensity();

        Debug.Log("Shotgun fired!");

        for (int i = 0; i < pelletsPerShot; i++)
        {
            FirePellet();
        }

        // Play muzzle flash effect
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    private void FirePellet()
    {
        Ray ray = new Ray(playerCamera.transform.position, GetSpreadDirection());
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Shotgun pellet hit: " + hit.collider.name);

            EntityHealth entity = hit.collider.GetComponent<EntityHealth>();
            if (entity != null)
            {
                entity.TakeDamageServerRpc(damage / pelletsPerShot);

                // Spawn blood effect
                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BloodImpact");
                }
            }
            else
            {
                // Spawn regular impact effect
                if (NetworkImpactSpawner.Instance != null)
                {
                    NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "BulletImpact");
                }
            }
        }
    }

    private Vector3 GetSpreadDirection()
    {
        // Generate random spread within cone
        float spreadX = Random.Range(-spreadAngle, spreadAngle);
        float spreadY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(spreadY, spreadX, 0);
        return spreadRotation * playerCamera.transform.forward;
    }
}
