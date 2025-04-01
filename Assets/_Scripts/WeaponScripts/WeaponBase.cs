using UnityEngine;
using System.Collections.Generic;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int maxAmmo;
    public int currentAmmo;
    public int reserveAmmo;

    [Header("Weapon Settings")]
    public bool isAutomatic;
    public float fireRate;
    public float reloadDuration;

    [Header("Shoot Settings")]
    public float range;
    public int damage;

    [Header("Effects")]
    public Transform muzzleTransform;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffectPrefab;

    [Header("Emission")]
    [SerializeField] private List<Renderer> weaponRenderers = new();
    [SerializeField] private Color baseEmissionColor = new Color(0f, 255f / 255f, 255f / 255f);

    protected Camera playerCamera;
    protected NetworkImpactSpawner networkImpactSpawner;

    protected virtual void Start()
    {
        playerCamera = Camera.main;
        networkImpactSpawner = FindFirstObjectByType<NetworkImpactSpawner>();
        
        currentAmmo = maxAmmo;

        UpdateEmissionIntensity();
    }

    public bool CanShoot() => currentAmmo > 0;
    public bool CanReload() => currentAmmo < maxAmmo && reserveAmmo > 0;

    // Abstract shoot method to be implemented by each weapon type.
    public abstract void Shoot();

    public void UpdateEmissionIntensity()
    {
        if (weaponRenderers == null || weaponRenderers.Count == 0 || maxAmmo == 0) return;

        float ammoRatio = (float)currentAmmo / maxAmmo;
        float intensity = Mathf.Lerp(-4f, 6f, ammoRatio);
        Color finalColor = baseEmissionColor * Mathf.LinearToGammaSpace(intensity);

        foreach (var renderer in weaponRenderers)
        {
            if (renderer != null)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", finalColor);
            }
        }
    }
}
