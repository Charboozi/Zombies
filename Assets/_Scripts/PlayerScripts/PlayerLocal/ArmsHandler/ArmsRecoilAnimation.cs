using UnityEngine;
using System.Collections;

public class ArmsRecoilAnimation : MonoBehaviour, IArmsOffsetProvider
{
    public float recoilAmount = 0.1f;
    public float rotationAmount = 5f; // degrees of upward tilt
    public float recoilDuration = 0.08f;
    public float recoveryDuration = 0.12f;

    private Vector3 recoilOffset = Vector3.zero;
    private Quaternion recoilRotation = Quaternion.identity;
    private Coroutine recoilCoroutine;

    void OnEnable() => WeaponController.OnShoot += HandleRecoil;
    void OnDisable() => WeaponController.OnShoot -= HandleRecoil;

    private void HandleRecoil()
    {
        if (recoilCoroutine != null)
            StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(RecoilRoutine());
    }

    private IEnumerator RecoilRoutine()
    {
        var weapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        float multiplier = weapon != null ? weapon.recoilStrength/1.5f : 1f;

        float finalAmount = recoilAmount * multiplier;
        float finalRotation = rotationAmount * multiplier;

        Vector3 startOffset = new(0, 0, -finalAmount);
        Quaternion startRotation = Quaternion.Euler(-finalRotation, 0f, Random.Range(-finalRotation * 0.25f, finalRotation * 0.25f));

        float t = 0f;
        while (t < recoilDuration)
        {
            t += Time.deltaTime;
            float factor = t / recoilDuration;
            recoilOffset = Vector3.Lerp(Vector3.zero, startOffset, factor);
            recoilRotation = Quaternion.Slerp(Quaternion.identity, startRotation, factor);
            yield return null;
        }

        t = 0f;
        while (t < recoveryDuration)
        {
            t += Time.deltaTime;
            float factor = t / recoveryDuration;
            recoilOffset = Vector3.Lerp(startOffset, Vector3.zero, factor);
            recoilRotation = Quaternion.Slerp(startRotation, Quaternion.identity, factor);
            yield return null;
        }

        recoilOffset = Vector3.zero;
        recoilRotation = Quaternion.identity;
    }

    public Vector3 GetOffset() => recoilOffset;
    public Quaternion GetRotation() => recoilRotation;
}
