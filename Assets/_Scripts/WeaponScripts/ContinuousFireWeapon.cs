using UnityEngine;
using System.Collections;

public class ContinuousFireWeapon : WeaponBase
{
    [Header("Continuous Fire Settings")]
    [SerializeField] private float initialFireDelay = 0.3f;
    public float damageInterval = 0.1f; // Time between damage ticks
    public float splashRadius = 2f;     // Radius of splash damage

    private bool isFiring = false;
    private Coroutine fireCoroutine;
    

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        // Start firing on initial button press.
        if (Input.GetMouseButtonDown(0) && CanShoot())
        {
            StartFiring();
        }
        // Stop firing when button is released or when we can't shoot.
        if (!Input.GetMouseButton(0) || !CanShoot() || !muzzleFlash.isPlaying)
        {
            StopFiring();
        }
    }

    private void StartFiring()
    {
        if (isFiring) return; // Prevent duplicate coroutines.
        
        // Start the muzzle flash effect if available.
        if (muzzleFlash != null && !muzzleFlash.isPlaying)
        {
            muzzleFlash.Play();
        }
        
        isFiring = true;
        fireCoroutine = StartCoroutine(FireContinuously());

    }

    private void StopFiring()
    {
        if (!isFiring) return;
        isFiring = false;
        // Stop the muzzle flash effect.
        if (muzzleFlash != null && muzzleFlash.isPlaying)
        {
            muzzleFlash.Stop();
        }
        if (fireCoroutine != null)
        {
            StopCoroutine(fireCoroutine);
            fireCoroutine = null;
        }

    }

    private IEnumerator FireContinuously()
    {
        yield return new WaitForSeconds(initialFireDelay);

        while (isFiring && currentAmmo > 0 && CanShoot())
        {
            Shoot();
            yield return new WaitForSeconds(damageInterval);
        }

        StopFiring();
    }

    public override void Shoot()
    {
        if (!CanShoot()) return;

        // Deduct ammo for this shot.
        currentAmmo--;

        UpdateEmissionIntensity();

        // Raycast forward from the player camera.
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            Debug.Log("Continuous Fire Hit: " + hit.collider.name);

            // Apply splash damage at the hit point.
            ApplySplashDamage(hit.point);

            // Optionally, spawn a networked impact effect.
            // if (networkImpactSpawner != null)
            // {
            //     networkImpactSpawner.SpawnImpactEffectServerRpc(hit.point, hit.normal, impactEffectPrefab.name);
            // }
        }
    }

    private void ApplySplashDamage(Vector3 center)
    {
        Collider[] hitObjects = Physics.OverlapSphere(center, splashRadius);
        foreach (Collider obj in hitObjects)
        {
            EntityHealth entity = obj.GetComponent<EntityHealth>();
            if (entity != null)
            {
                entity.TakeDamageServerRpc(damage);
            }
        }
    }
}
