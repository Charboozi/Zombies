using UnityEngine;
using System.Collections;

public class ArmsReloadAnimation : MonoBehaviour, IArmsOffsetProvider
{
    public float reloadAmount = 0.2f;

    private Vector3 reloadOffset = Vector3.zero;
    private Coroutine reloadCoroutine;

    void OnEnable() => WeaponController.OnReloadStart += StartReload;
    void OnDisable() => WeaponController.OnReloadStart -= StartReload;

    private void StartReload()
    {
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
        reloadCoroutine = StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        WeaponBase weapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        if (weapon == null) yield break;

        float dur = weapon.reloadDuration;
        float half = dur / 2f;

        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            reloadOffset = Vector3.Lerp(Vector3.zero, new Vector3(0, -reloadAmount, 0), t / half);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            reloadOffset = Vector3.Lerp(new Vector3(0, -reloadAmount, 0), Vector3.zero, t / half);
            yield return null;
        }

        reloadOffset = Vector3.zero;
    }

    public Vector3 GetOffset() => reloadOffset;
}
