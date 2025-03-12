using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    
    [Header("Ammo Settings")]
    public int maxAmmo = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 90;
    
    public Transform muzzleTransform;
    public ParticleSystem muzzleFlash;
    
    [Header("Shoot Settings")]
    public float range = 100f;   // Maximum shooting distance
    public int damage = 10;
    public bool isAutomatic = false; 
    public float fireRate = 0.1f; //For automatic weapons


    [Header("Effects")]
    public GameObject impactEffectPrefab; 

    private Camera playerCamera;
    private NetworkImpactSpawner networkImpactSpawner;

    void Start()
    {
        playerCamera = Camera.main;
        networkImpactSpawner = FindFirstObjectByType<NetworkImpactSpawner>();
    }

    public bool CanShoot()
    {
        return currentAmmo > 0;
    }

    public bool CanReload()
    {
        return currentAmmo < maxAmmo && reserveAmmo > 0;
    }

    public void Shoot()
    {
        if (currentAmmo <= 0 && !gameObject.activeInHierarchy)
            return;

        currentAmmo--;

        
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.name);

            EntityHealth entity = hit.collider.GetComponent<EntityHealth>();

            // Example: Apply damage if the object has a script with a TakeDamage method
            if (entity != null)
            {
                entity.TakeDamageServerRpc(damage);
            }

            networkImpactSpawner.SpawnImpactEffectServerRpc(hit.point, hit.normal, impactEffectPrefab.name);
        }
        
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    public void Reload()
    {
        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;
    }
}