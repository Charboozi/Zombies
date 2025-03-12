using UnityEngine;
using System.Collections;

public class ProceduralWeaponAnimation : MonoBehaviour
{
    private WeaponStats weaponStats;
    private WeaponSwitcher weaponSwitcher;

    [Header("Reload Settings")]
    public float reloadLowerAmount = 0.2f; // How much the gun lowers during reload
    public float reloadSpeed = 6f; // Speed of reload animation
    public float reloadDuration = 1.2f; // How long reload takes

    [Header("Recoil Settings")]
    public float recoilAmount = 0.1f; // How much the gun moves backward when shooting
    public float recoilSpeed = 10f; // Speed of the recoil effect
    public float recoilRecoverySpeed = 8f; // Speed of return to normal position

    private Vector3 originalPosition;
    private bool isReloading = false;
    private bool isRecoiling = false;
    private float nextFireTime = 0f;

    public event System.Action OnShoot;

    void Start()
    {
        weaponSwitcher = GetComponent<WeaponSwitcher>();

        // Store original weapon position
        originalPosition = transform.localPosition;

        weaponSwitcher.OnWeaponSwitched += AssignWeaponStats;
    }

    public void AssignWeaponStats()
    {
        weaponStats = GetActiveWeaponChild().GetComponent<WeaponStats>();

        if (weaponStats == null)
        {
            Debug.LogWarning($"No WeaponStats found on {gameObject.name}!", gameObject);
        }
    }

    GameObject GetActiveWeaponChild()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.activeSelf) // Check if the child is active
            {
                return child.gameObject; // Return the active child
            }
        }
        return null; // Return null if no active child is found
    }

    void OnDestroy()
    {
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnWeaponSwitched -= AssignWeaponStats;
        }
    }

    void Update()
    {
        HandleReload();
        HandleShooting();
    }

    private void HandleReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && weaponStats != null && weaponStats.CanReload()) // Press 'R' to reload
        {
            StartCoroutine(PerformReload());
        }
    }

    private IEnumerator PerformReload()
    {
        isReloading = true;

        // Lower the weapon down smoothly
        Vector3 loweredPosition = originalPosition - new Vector3(0, reloadLowerAmount, 0);
        float timer = 0f;

        while (timer < reloadDuration / 2f) // Lowering phase
        {
            timer += Time.deltaTime * reloadSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, loweredPosition, Time.deltaTime * reloadSpeed);
            yield return null;
        }

        // **Ammo refills exactly when the weapon starts rising up**
        if (weaponStats != null)
        {
            weaponStats.Reload();
            isReloading = false;
        }

        // Raising phase: smoothly return the gun to its original position
        timer = 0f;
        while (timer < reloadDuration / 2f) // Raising phase
        {
            timer += Time.deltaTime * reloadSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * reloadSpeed);
            yield return null;
        }
    }


    private void HandleShooting()
    {
        if (weaponStats == null || isReloading || isRecoiling) return; // Prevent shooting if conditions aren't met

        // Semi-Automatic: Fire on mouse button press
        if (!weaponStats.isAutomatic && Input.GetMouseButtonDown(0) && weaponStats.CanShoot())
        {
            StartCoroutine(PerformRecoil());
            weaponStats.Shoot();
            OnShoot?.Invoke();
        }
        // Automatic: Fire continuously while holding the button
        else if (weaponStats.isAutomatic && Input.GetMouseButton(0) && weaponStats.CanShoot())
        {
            if (Time.time >= nextFireTime) // Fire only when cooldown allows
            {
                nextFireTime = Time.time + weaponStats.fireRate; // Set next fire time
                StartCoroutine(PerformRecoil());
                weaponStats.Shoot();
                OnShoot?.Invoke();
            }
        }
    }

    private IEnumerator PerformRecoil()
    {
        isRecoiling = true;

        // Move the weapon back to simulate recoil
        Vector3 recoilPosition = originalPosition - new Vector3(0, 0, recoilAmount);
        float timer = 0f;

        while (timer < 0.1f) // Quick recoil backward
        {
            timer += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPosition, Time.deltaTime * recoilSpeed);
            yield return null;
        }

        // Recover back to original position
        timer = 0f;
        while (timer < 0.1f) 
        {
            timer += Time.deltaTime * recoilRecoverySpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilRecoverySpeed);
            yield return null;
        }

        isRecoiling = false;
    }
}
