using UnityEngine;
using System.Collections;

public class ProceduralWeaponAnimation : MonoBehaviour
{
    // Instead of having our own weaponStats reference, we use WeaponManager to access the active weapon.
    private WeaponBase currentWeapon
    {
        get { return WeaponManager.Instance != null ? WeaponManager.Instance.CurrentWeapon : null; }
    }

    [Header("Reload Settings")]
    public float reloadLowerAmount = 0.2f;   // How much the gun lowers during reload
    public float reloadSpeed = 6f;           // Speed of reload animation

    [Header("Recoil Settings")]
    public float recoilAmount = 0.1f;        // How much the gun moves backward when shooting
    public float recoilSpeed = 10f;          // Speed of the recoil effect
    public float recoilRecoverySpeed = 8f;   // Speed of return to normal position

    private Vector3 originalPosition;
    private bool isReloading = false;
    private bool isRecoiling = false;
    private float nextFireTime = 0f;

    public event System.Action OnShoot;
    public event System.Action OnStopShooting; // For continous fire weapons

    void Start()
    {
        // Store the original local position for recoil and reload animations.
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        HandleReload();
        HandleShooting();
    }

    private void HandleReload()
    {
        // Press 'R' to reload if not already reloading and if the current weapon can reload.
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentWeapon != null && currentWeapon.CanReload())
        {
            StartCoroutine(PerformReload());
        }
    }

    private IEnumerator PerformReload()
    {
        isReloading = true;

        float reloadDuration = currentWeapon != null ? currentWeapon.reloadDuration : 1.2f;

        // Lower the weapon smoothly.
        Vector3 loweredPosition = originalPosition - new Vector3(0, reloadLowerAmount, 0);
        float timer = 0f;
        while (timer < reloadDuration / 2f)
        {
            timer += Time.deltaTime * reloadSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, loweredPosition, Time.deltaTime * reloadSpeed);
            yield return null;
        }

        // Call the reload method on the current weapon.
        if (currentWeapon != null)
        {
            currentWeapon.Reload();
        }
        isReloading = false;

        // Raise the weapon back to its original position.
        timer = 0f;
        while (timer < reloadDuration / 2f)
        {
            timer += Time.deltaTime * reloadSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * reloadSpeed);
            yield return null;
        }
    }

    private void HandleShooting()
    {
        if (currentWeapon == null || isReloading || isRecoiling) return; // ✅ Block shooting during reload
        if (Time.time < nextFireTime) return; // ✅ Prevent spam clicking

        if (!currentWeapon.isAutomatic && Input.GetMouseButtonDown(0) && currentWeapon.CanShoot())
        {
            nextFireTime = Time.time + currentWeapon.fireRate;
            StartCoroutine(PerformRecoil());
            currentWeapon.Shoot();
            OnShoot?.Invoke();
        }
        else if (currentWeapon.isAutomatic && Input.GetMouseButton(0) && currentWeapon.CanShoot())
        {
            nextFireTime = Time.time + currentWeapon.fireRate;
            StartCoroutine(PerformRecoil());
            currentWeapon.Shoot();
            OnShoot?.Invoke();
        }
        else if (currentWeapon is ContinuousFireWeapon && Input.GetMouseButtonUp(0))
        {
            OnStopShooting?.Invoke(); // Notify when stopping continuous fire
        }
    }


    private IEnumerator PerformRecoil()
    {
        isRecoiling = true;
        // Recoil backward.
        Vector3 recoilPosition = originalPosition - new Vector3(0, 0, recoilAmount);
        float timer = 0f;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPosition, Time.deltaTime * recoilSpeed);
            yield return null;
        }
        // Recover recoil.
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
