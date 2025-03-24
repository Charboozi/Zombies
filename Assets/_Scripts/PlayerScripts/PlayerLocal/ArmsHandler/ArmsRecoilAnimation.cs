using UnityEngine;
using System.Collections;

public class ArmsRecoilAnimation : MonoBehaviour, IArmsOffsetProvider
{
    public float recoilAmount = 0.1f;
    public float recoilDuration = 0.08f;
    public float recoveryDuration = 0.12f;

    private Vector3 recoilOffset = Vector3.zero;
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
        float t = 0f;
        while (t < recoilDuration)
        {
            t += Time.deltaTime;
            float factor = t / recoilDuration;
            recoilOffset = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, -recoilAmount), factor);
            yield return null;
        }

        t = 0f;
        while (t < recoveryDuration)
        {
            t += Time.deltaTime;
            float factor = t / recoveryDuration;
            recoilOffset = Vector3.Lerp(new Vector3(0, 0, -recoilAmount), Vector3.zero, factor);
            yield return null;
        }

        recoilOffset = Vector3.zero;
    }

    public Vector3 GetOffset() => recoilOffset;
}
