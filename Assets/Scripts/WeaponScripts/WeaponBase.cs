using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int maxAmmo;
    public int currentAmmo;
    public int reserveAmmo;

    [Header("Weapon Settings")]
    public bool isAutomatic;
    public float fireRate;

    [Header("Shoot Settings")]
    public float range;
    public int damage;

    [Header("Effects")]
    public Transform muzzleTransform;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffectPrefab;

    protected Camera playerCamera;
    protected NetworkImpactSpawner networkImpactSpawner;

    protected virtual void Start()
    {
        // Use the main camera if not set otherwise
        playerCamera = Camera.main;
        networkImpactSpawner = FindFirstObjectByType<NetworkImpactSpawner>();

        // Optionally, set current ammo to max on start
        currentAmmo = maxAmmo;
    }

    public bool CanShoot()
    {
        return currentAmmo > 0;
    }

    public bool CanReload()
    {
        return currentAmmo < maxAmmo && reserveAmmo > 0;
    }

    public virtual void Reload()
    {
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;
    }

    // Abstract shoot method to be implemented by each weapon type.
    public abstract void Shoot();
}
