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

    [Header("Audio")]
    [SerializeField] private AudioSource chargingLoopAudioSource;
    [SerializeField] private AudioClip chargingLoopClip;

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

        if (chargingLoopAudioSource != null && chargingLoopClip != null)
        {
            if (fadeOutCoroutine != null)
                StopCoroutine(fadeOutCoroutine);

            chargingLoopAudioSource.clip = chargingLoopClip;
            chargingLoopAudioSource.volume = 0f;
            chargingLoopAudioSource.loop = true;
            chargingLoopAudioSource.Play();
            StartCoroutine(FadeInAudio(chargingLoopAudioSource, 0.2f)); // Fade in over 0.5s
        }

        while (currentWeapon.currentAmmo < currentWeapon.maxAmmo && currentWeapon.reserveAmmo > 0)
        {
            yield return new WaitForSeconds(0.1f);
            currentWeapon.currentAmmo++;
            currentWeapon.reserveAmmo--;
            currentWeapon.UpdateEmissionIntensity();
        }
        
        OnReloadEnd?.Invoke();
        autoReloadCoroutine = null;
        StopReloading();
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
        if (chargingLoopAudioSource != null && chargingLoopAudioSource.isPlaying)
        {
            if (fadeOutCoroutine != null)
                StopCoroutine(fadeOutCoroutine);

            fadeOutCoroutine = StartCoroutine(FadeOutAudio(chargingLoopAudioSource, 0.5f)); // 0.5s fade
        }

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


    private Coroutine fadeOutCoroutine;
    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        float startVolume = source.volume;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        source.Stop();
        source.clip = null;
        source.volume = startVolume; // Reset volume
    }

    private IEnumerator FadeInAudio(AudioSource source, float duration)
    {
        float t = 0f;
        float targetVolume = 0.2f;

        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

}
