using UnityEngine;
using System.Collections;

public class ProceduralWeaponAnimation : MonoBehaviour
{
    [Header("Reload Settings")]
    public float reloadLowerAmount = 0.2f;
    public float reloadLerpDuration = 0.25f; // How long to lower/raise

    [Header("Recoil Settings")]
    public float recoilAmount = 0.1f;
    public float recoilDuration = 0.08f;
    public float recoilRecoveryDuration = 0.12f;

    private Vector3 defaultPosition;
    private Coroutine recoilCoroutine;
    private Coroutine reloadCoroutine;

    private void Start()
    {
        defaultPosition = transform.localPosition;

        WeaponController.OnShoot += StartRecoil;
        WeaponController.OnReloadStart += StartReloadAnimation;
    }

    private void OnDestroy()
    {
        WeaponController.OnShoot -= StartRecoil;
        WeaponController.OnReloadStart -= StartReloadAnimation;
    }

    private void StartRecoil()
    {
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);

        recoilCoroutine = StartCoroutine(HandleRecoil());
    }

    private IEnumerator HandleRecoil()
    {
        Vector3 startPos = transform.localPosition;
        Vector3 recoilPos = startPos - new Vector3(0, 0, recoilAmount);
        float t = 0f;

        while (t < recoilDuration)
        {
            t += Time.deltaTime;
            float lerpFactor = Mathf.Clamp01(t / recoilDuration);
            transform.localPosition = Vector3.Lerp(startPos, recoilPos, lerpFactor);
            yield return null;
        }

        t = 0f;
        while (t < recoilRecoveryDuration)
        {
            t += Time.deltaTime;
            float lerpFactor = Mathf.Clamp01(t / recoilRecoveryDuration);
            transform.localPosition = Vector3.Lerp(recoilPos, defaultPosition, lerpFactor);
            yield return null;
        }

        transform.localPosition = defaultPosition;
    }

    private void StartReloadAnimation()
    {
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);

        reloadCoroutine = StartCoroutine(HandleReloadAnimation());
    }

    private IEnumerator HandleReloadAnimation()
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        if (weapon == null)
        {
            Debug.LogWarning("No current weapon found for reload animation.");
            yield break;
        }

        float reloadDuration = weapon.reloadDuration;
        float halfDuration = reloadDuration / 2f;

        Vector3 loweredPos = defaultPosition - new Vector3(0, reloadLowerAmount, 0);

        // Lower weapon over first half
        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float lerpFactor = Mathf.Clamp01(t / halfDuration);
            transform.localPosition = Vector3.Lerp(defaultPosition, loweredPos, lerpFactor);
            yield return null;
        }

        // Raise weapon over second half
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float lerpFactor = Mathf.Clamp01(t / halfDuration);
            transform.localPosition = Vector3.Lerp(loweredPos, defaultPosition, lerpFactor);
            yield return null;
        }

        transform.localPosition = defaultPosition;
    }

}
