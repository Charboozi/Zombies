using UnityEngine;
using System;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    private WeaponBase currentWeapon => CurrentWeaponHolder.Instance.CurrentWeapon;

    private float nextFireTime = 0f;
    private bool isFiringHeld = false;
    private Coroutine autoReloadCoroutine = null;
    private Coroutine delayBeforeReloadCoroutine = null;

    public static event Action OnShoot;
    public static event Action OnStopShooting;
    public static event Action OnReloadStart;
    public static event Action OnReloadEnd;

    private void OnEnable()
    {
        PlayerInput.OnFirePressed += HandleFirePressed;
        PlayerInput.OnFireReleased += HandleFireReleased;
    }

    private void OnDisable()
    {
        PlayerInput.OnFirePressed -= HandleFirePressed;
        PlayerInput.OnFireReleased -= HandleFireReleased;
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        if (!currentWeapon.HandlesInput) // ✅ Skip automatic fire if weapon handles its own input
        {
            if (isFiringHeld && currentWeapon.isAutomatic && Time.time >= nextFireTime && currentWeapon.CanShoot())
            {
                FireWeapon();
            }
        }

        if (!isFiringHeld && !IsReloading && currentWeapon.currentAmmo < currentWeapon.maxAmmo && delayBeforeReloadCoroutine == null)
        {
            delayBeforeReloadCoroutine = StartCoroutine(DelayBeforeReloadCoroutine());
        }
    }


    private void HandleFirePressed()
    {
        if (currentWeapon == null || !currentWeapon.CanShoot()) return;

        isFiringHeld = true;
        StopReloading();

        if (currentWeapon.HandlesInput) return; // ✅ Skip if weapon handles its own input

        if (!currentWeapon.isAutomatic && Time.time >= nextFireTime)
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

    private void FireWeapon()
    {
        nextFireTime = Time.time + currentWeapon.fireRate;
        currentWeapon.Shoot();
        OnShoot?.Invoke();
    }

    private IEnumerator DelayBeforeReloadCoroutine()
    {
        float interval = currentWeapon.reloadDuration / currentWeapon.maxAmmo;
        yield return new WaitForSeconds(interval);
        autoReloadCoroutine = StartCoroutine(AutoReloadCoroutine());
        delayBeforeReloadCoroutine = null;
    }
    private IEnumerator AutoReloadCoroutine()
    {
        OnReloadStart?.Invoke();

        while (currentWeapon.currentAmmo < currentWeapon.maxAmmo && currentWeapon.reserveAmmo > 0)
        {
            yield return new WaitForSeconds(0.1f);
            currentWeapon.currentAmmo++;
            currentWeapon.reserveAmmo--;
            currentWeapon.UpdateEmissionIntensity();
        }

        OnReloadEnd?.Invoke();
        autoReloadCoroutine = null;
    }

    public void OnWeaponSwitched()
    {
        StopReloading();

        // Check if new weapon needs reload
        if (currentWeapon != null && currentWeapon.currentAmmo < currentWeapon.maxAmmo)
        {
            delayBeforeReloadCoroutine = StartCoroutine(DelayBeforeReloadCoroutine());
        }
    }

    private void StopReloading()
    {
        if (delayBeforeReloadCoroutine != null)
        {
            StopCoroutine(delayBeforeReloadCoroutine);
            delayBeforeReloadCoroutine = null;
        }

        if (autoReloadCoroutine != null)
        {
            StopCoroutine(autoReloadCoroutine);
            autoReloadCoroutine = null;
            OnReloadEnd?.Invoke();
        }
    }

    public bool IsReloading => autoReloadCoroutine != null;
}
