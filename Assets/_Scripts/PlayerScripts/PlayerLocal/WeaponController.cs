using UnityEngine;
using System;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    private WeaponBase currentWeapon => CurrentWeaponHolder.Instance.CurrentWeapon;

    private float nextFireTime = 0f;
    private bool isReloading = false;
    private bool isFiringHeld = false;

    public static event Action OnShoot;
    public static event Action OnStopShooting;
    public static event Action OnReloadStart;
    public static event Action OnReloadEnd;

    private void OnEnable()
    {
        PlayerInput.OnFirePressed += HandleFirePressed;
        PlayerInput.OnFireReleased += HandleFireReleased;
        PlayerInput.OnReloadPressed += HandleReloadPressed;
    }

    private void OnDisable()
    {
        PlayerInput.OnFirePressed -= HandleFirePressed;
        PlayerInput.OnFireReleased -= HandleFireReleased;
        PlayerInput.OnReloadPressed -= HandleReloadPressed;
    }

    private void Update()
    {
        if (currentWeapon == null || isReloading) return;

        if (isFiringHeld && currentWeapon.isAutomatic && Time.time >= nextFireTime && currentWeapon.CanShoot())
        {
            FireWeapon();
        }
    }

    private void HandleFirePressed()
    {
        if (currentWeapon == null || isReloading) return;

        isFiringHeld = true;

        if (!currentWeapon.isAutomatic && Time.time >= nextFireTime && currentWeapon.CanShoot())
        {
            FireWeapon();
        }
    }

    private void HandleFireReleased()
    {
        isFiringHeld = false;

        if (currentWeapon is ContinuousFireWeapon)
        {
            OnStopShooting?.Invoke();
        }
    }

    private void HandleReloadPressed()
    {
        if (isReloading || currentWeapon == null || !currentWeapon.CanReload()) return;

        StartCoroutine(HandleReload());
    }

    private void FireWeapon()
    {
        nextFireTime = Time.time + currentWeapon.fireRate;
        currentWeapon.Shoot();
        OnShoot?.Invoke();
    }

    private IEnumerator HandleReload()
    {
        isReloading = true;
        OnReloadStart?.Invoke();

        yield return new WaitForSeconds(currentWeapon.reloadDuration);

        currentWeapon.Reload();
        isReloading = false;
        OnReloadEnd?.Invoke();
    }

    public bool IsReloading => isReloading;
}
