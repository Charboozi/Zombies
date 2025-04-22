using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles ambient or persistent screen tints that shouldn't be overridden by momentary effects.
/// </summary>
public class PersistentScreenTint : MonoBehaviour
{
    public static PersistentScreenTint Instance;

    private Image screenImage;
    private Coroutine activeEffect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        screenImage = GetComponent<Image>();
        screenImage.color = new Color(0, 0, 0, 0); // transparent by default
    }

    /// <summary>
    /// Sets a persistent tint that lasts for a given duration, then fades out automatically.
    /// </summary>
    public void SetPersistentTintForDuration(Color color, float duration, float alpha = 0.04f, float fadeSpeed = 0.1f)
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = StartCoroutine(PersistentTintDurationRoutine(color, duration, alpha, fadeSpeed));
    }

    private IEnumerator PersistentTintDurationRoutine(Color color, float duration, float alpha, float fadeSpeed)
    {
        // Fade in
        float a = 0f;
        while (a < alpha)
        {
            a += Time.deltaTime * fadeSpeed;
            screenImage.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(a));
            yield return null;
        }

        // Hold
        screenImage.color = new Color(color.r, color.g, color.b, alpha);
        yield return new WaitForSeconds(duration);

        // Fade out
        while (a > 0f)
        {
            a -= Time.deltaTime * fadeSpeed;
            screenImage.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(a));
            yield return null;
        }

        screenImage.color = new Color(0, 0, 0, 0);
        activeEffect = null;
    }


    /// <summary>
    /// Fade in persistent tint (ambient), used for atmospheric effects.
    /// </summary>
    public void FadeInPersistentTint(Color color, float targetAlpha = 0.04f, float speed = 0.1f)
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = StartCoroutine(FadeInRoutine(color, targetAlpha, speed));
    }

    private IEnumerator FadeInRoutine(Color color, float targetAlpha, float speed)
    {
        float alpha = 0f;
        while (alpha < targetAlpha)
        {
            alpha += Time.deltaTime * speed;
            screenImage.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
            yield return null;
        }

        screenImage.color = new Color(color.r, color.g, color.b, targetAlpha);
        activeEffect = null;
    }

    /// <summary>
    /// Clear the persistent tint (fade to transparent).
    /// </summary>
    public void ClearPersistentTint(float fadeSpeed = 0.1f)
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = StartCoroutine(FadeOutRoutine(fadeSpeed));
    }

    private IEnumerator FadeOutRoutine(float speed)
    {
        Color color = screenImage.color;
        float alpha = color.a;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * speed;
            screenImage.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
            yield return null;
        }

        screenImage.color = new Color(0, 0, 0, 0);
        activeEffect = null;
    }
}
