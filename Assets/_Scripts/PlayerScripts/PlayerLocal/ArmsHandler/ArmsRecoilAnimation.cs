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
        float multiplier = weapon != null ? weapon.recoilStrength / 1.5f : 1f;

        float finalAmount = recoilAmount * multiplier;
        float finalRotation = rotationAmount * multiplier;

        float snapDuration = 0.04f;
        float recoveryDurationDynamic = Mathf.Lerp(0.08f, 0.18f, Mathf.Clamp01((weapon?.recoilStrength ?? 1f) / 2f));

        float snapBoost = 1.4f; // optional: increase for punchier recoil

        Vector3 startOffset = new(0, 0, -finalAmount * snapBoost);
        Quaternion startRotation = Quaternion.Euler(
            -finalRotation * snapBoost,
            0f,
            Random.Range(-finalRotation * 0.25f, finalRotation * 0.25f)
        );

        // 💥 Instant frame-1 jolt
        recoilOffset = startOffset;
        recoilRotation = startRotation;
        yield return null;

        // 🔹 Snap phase with ease-out (sharp acceleration)
        float SnapEaseOut(float x) => 1f - Mathf.Cos((x * Mathf.PI) / 2f);

        float t = 0f;
        while (t < snapDuration)
        {
            t += Time.deltaTime;
            float factor = SnapEaseOut(t / snapDuration);
            recoilOffset = Vector3.Lerp(Vector3.zero, startOffset, factor);
            recoilRotation = Quaternion.Slerp(Quaternion.identity, startRotation, factor);
            yield return null;
        }

        // 🔸 Smooth recovery
        t = 0f;
        while (t < recoveryDurationDynamic)
        {
            t += Time.deltaTime;
            float factor = t / recoveryDurationDynamic;
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
