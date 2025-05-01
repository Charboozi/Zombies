using UnityEngine;

public class HealingWeapon : WeaponBase
{
    [Header("Healing Settings")]
    [SerializeField] private float baseSpreadAngle = 0f;
    [SerializeField] private int healingAmount = 10;
    [SerializeField] private bool canHealDownedPlayers = false;


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

        Vector3 direction = playerCamera.transform.forward;

        if (baseSpreadAngle > 0f)
        {
            direction = ApplySpread(direction);
        }

        Ray ray = new Ray(playerCamera.transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent(out EntityHealth entity))
            {
                if (entity.CompareTag("Player"))
                {
                    bool isDowned = entity.isDowned.Value;
                    bool canHealThis = !isDowned || (isDowned && canHealDownedPlayers);

                    if (canHealThis && entity.currentHealth.Value < entity.maxHealth)
                    {
                        entity.ApplyHealingServerRpc(healingAmount);

                        if (NetworkImpactSpawner.Instance != null)
                        {
                            NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "HealImpact");
                        }

                        if (!canPierceEnemies)
                            break;

                        continue;
                    }
                }
            }

            if (NetworkImpactSpawner.Instance != null)
            {
                NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, "HealImpact");
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
