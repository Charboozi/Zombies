using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

/// <summary>
/// Handles ambient or persistent screen tints via the vignette post-processing effect.
/// Suitable for atmospheric overlays that persist and blend subtly.
/// </summary>
public class PersistentScreenTint : MonoBehaviour
{
    public static PersistentScreenTint Instance;

    [SerializeField] private Volume globalVolume;
    private Vignette vignette;
    private Coroutine activeEffect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (globalVolume == null)
        {
            Debug.LogError("PersistentScreenTint: Global Volume not assigned.");
            return;
        }

        if (!globalVolume.profile.TryGet(out vignette))
        {
            Debug.LogError("PersistentScreenTint: Vignette not found in Global Volume profile.");
        }
    }

    /// <summary>
    /// Sets a persistent vignette tint for a given duration, then fades out.
    /// </summary>
    public void SetPersistentTintForDuration(Color color, float duration, float intensity = 0.04f, float fadeSpeed = 0.1f)
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = StartCoroutine(PersistentTintDurationRoutine(color, duration, intensity, fadeSpeed));
    }

    private IEnumerator PersistentTintDurationRoutine(Color color, float duration, float intensity, float fadeSpeed)
    {
        vignette.active = true;
        vignette.color.value = color;

        // Fade in
        float i = 0f;
        while (i < intensity)
        {
            i += Time.deltaTime * fadeSpeed;
            vignette.intensity.value = Mathf.Clamp01(i);
            yield return null;
        }

        vignette.intensity.value = intensity;

        // Hold
        yield return new WaitForSeconds(duration);

        // Fade out
        while (i > 0f)
        {
            i -= Time.deltaTime * fadeSpeed;
            vignette.intensity.value = Mathf.Clamp01(i);
            yield return null;
        }

        vignette.intensity.value = 0f;
        vignette.active = false;
        activeEffect = null;
    }

    /// <summary>
    /// Fade in a persistent vignette tint for ambient effects.
    /// </summary>
    public void FadeInPersistentTint(Color color, float targetIntensity = 0.04f, float speed = 0.1f)
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = StartCoroutine(FadeInRoutine(color, targetIntensity, speed));
    }

    private IEnumerator FadeInRoutine(Color color, float targetIntensity, float speed)
    {
        vignette.active = true;
        vignette.color.value = color;

        float i = vignette.intensity.value;
        while (i < targetIntensity)
        {
            i += Time.deltaTime * speed;
            vignette.intensity.value = Mathf.Clamp01(i);
            yield return null;
        }

        vignette.intensity.value = targetIntensity;
        activeEffect = null;
    }

    /// <summary>
    /// Clears the persistent vignette tint with a fade.
    /// </summary>
    public void ClearPersistentTint(float fadeSpeed = 0.1f)
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = StartCoroutine(FadeOutRoutine(fadeSpeed));
    }

    private IEnumerator FadeOutRoutine(float speed)
    {
        float i = vignette.intensity.value;

        while (i > 0f)
        {
            i -= Time.deltaTime * speed;
            vignette.intensity.value = Mathf.Clamp01(i);
            yield return null;
        }

        vignette.intensity.value = 0f;
        vignette.active = false;
        activeEffect = null;
    }
}
