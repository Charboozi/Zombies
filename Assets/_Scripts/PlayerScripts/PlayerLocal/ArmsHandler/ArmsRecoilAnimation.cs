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
        float strength = weapon != null 
            ? weapon.recoilStrength 
            : 1f;

        // Normalize strength (assuming maxRecoilStrength ~ 2.5f–3f)
        float norm = Mathf.Clamp01(strength / 2.5f);

        // Map norm → [0.8, 1.2] for low→high recoil
        float mult = Mathf.Lerp(0.8f, 1.2f, norm);

        float finalAmount   = recoilAmount  * mult;
        float finalRotation = rotationAmount * mult;

        // Make stronger recoil snap *faster*, but not too jarring:
        float snapDuration  = Mathf.Lerp(0.06f, 0.03f, norm);
        // Make recovery slower for strong recoil (smoother):
        float recoveryDur   = Mathf.Lerp(0.12f, 0.25f, norm);

        // First-frame jolt:
        Vector3 startOffset = new Vector3(0f, 0f, -finalAmount * 1.2f);
        Quaternion startRot = Quaternion.Euler(
            -finalRotation * 1.2f,
            0f,
            Random.Range(-finalRotation * 0.2f, finalRotation * 0.2f)
        );
        recoilOffset   = startOffset;
        recoilRotation = startRot;
        yield return null;

        // Snap phase (ease-out cosine)
        float t = 0f;
        while (t < snapDuration)
        {
            t += Time.deltaTime;
            float f = 1f - Mathf.Cos((t / snapDuration) * (Mathf.PI / 2f));
            recoilOffset   = Vector3.Lerp(Vector3.zero, startOffset, f);
            recoilRotation = Quaternion.Slerp(Quaternion.identity, startRot, f);
            yield return null;
        }

        // Smooth recovery with SmoothStep
        t = 0f;
        while (t < recoveryDur)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / recoveryDur);
            recoilOffset   = Vector3.Lerp(startOffset, Vector3.zero, f);
            recoilRotation = Quaternion.Slerp(startRot, Quaternion.identity, f);
            yield return null;
        }

        recoilOffset   = Vector3.zero;
        recoilRotation = Quaternion.identity;
    }




    public Vector3 GetOffset() => recoilOffset;
    public Quaternion GetRotation() => recoilRotation;
}
