using UnityEngine;
using System.Collections;

public class ContinuousFireWeapon : WeaponBase
{
    [Header("Continuous Fire Settings")]
    [SerializeField] private float initialChargeDelay = 1f;
    [SerializeField] private float damageInterval = 0.1f;
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private string impactEffect; 

    private Coroutine firingRoutine;

    public override bool HandlesInput => true;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && CanShoot())
        {
            // Start firing coroutine
            if (firingRoutine == null)
                firingRoutine = StartCoroutine(FireRoutine());
        }

        if (Input.GetMouseButtonUp(0) || !CanShoot())
        {
            StopFiring();
        }
    }

    private IEnumerator FireRoutine()
    {
        // Optional: Muzzle effect on charge start
        if (muzzleFlash != null && !muzzleFlash.isPlaying)
            muzzleFlash.Play();

        // Initial charge delay
        yield return new WaitForSeconds(initialChargeDelay);

        while (CanShoot() && Input.GetMouseButton(0))
        {
            Shoot();
            yield return new WaitForSeconds(damageInterval);
        }

        StopFiring();
    }

    private void StopFiring()
    {
        if (firingRoutine != null)
        {
            StopCoroutine(firingRoutine);
            firingRoutine = null;
        }

        if (muzzleFlash != null && muzzleFlash.isPlaying)
            muzzleFlash.Stop();
    }

    public override void Shoot()
    {
        if (!CanShoot() || currentAmmo <= 0) return;

        currentAmmo--;
        UpdateEmissionIntensity();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, range);
        foreach (RaycastHit hit in hits)
        {
            // Damage posters
            if (hit.collider.TryGetComponent(out FlammablePoster poster))
            {
                poster.ApplyBurnDamage(damageInterval); // small damage per tick
            }

            ApplySplashDamage(hit.point);

        if (NetworkImpactSpawner.Instance != null && !string.IsNullOrEmpty(impactEffect))
        {
            NetworkImpactSpawner.Instance.SpawnImpactEffectServerRpc(hit.point, hit.normal, impactEffect);
        }

            // If piercing is disabled, only hit the first target
            if (!canPierceEnemies)
                break;
        }
        
        WeaponController.Instance?.TriggerShootEffect();
    }

    private void ApplySplashDamage(Vector3 center)
    {
        Collider[] hitObjects = Physics.OverlapSphere(center, splashRadius);
        foreach (Collider obj in hitObjects)
        {
            if (obj.TryGetComponent(out EntityHealth entity))
            {
                if (entity.CompareTag("Player") && GameModeManager.Instance.IsPvPMode)
                continue;

                entity.TakeDamageServerRpc(damage);
            }
        }
    }
}
