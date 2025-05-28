using UnityEngine;
using System.Collections;

public class ContinuousFireWeapon : WeaponBase
{
    [Header("Continuous Fire Settings")]
    [SerializeField] private float initialChargeDelay = 1f;
    [SerializeField] private float damageInterval = 0.1f;
    [SerializeField] private float splashRadius = 2f;
    [SerializeField] private string impactEffect; 

    private PlayerControls input;
    private bool isFiring = false;

    private Coroutine firingRoutine;

    public override bool HandlesInput => true;

    private void OnEnable()
    {
        if (input == null)
        {
            input = new PlayerControls();
            input.Player.Fire.performed += ctx => StartFiring();
            input.Player.Fire.canceled += ctx => StopFiring();
        }
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }


    private IEnumerator FireRoutine()
    {
        if (muzzleFlash != null && !muzzleFlash.isPlaying)
            muzzleFlash.Play();

        yield return new WaitForSeconds(initialChargeDelay);

        while (CanShoot() && isFiring)
        {
            Shoot();
            yield return new WaitForSeconds(damageInterval);
        }

        StopFiring();
    }

    private void StartFiring()
    {
        if (!CanShoot() || firingRoutine != null)
            return;

        isFiring = true;
        firingRoutine = StartCoroutine(FireRoutine());
    }

    private void StopFiring()
    {
        isFiring = false;

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
                poster.BurnRequest(damageInterval);
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
