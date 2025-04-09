using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Show a fade screen effect. e.g. healing shows green, damage red, downed dark, etc.
/// </summary>
public class FadeScreenEffect : MonoBehaviour
{
    public static FadeScreenEffect Instance;

    private Image screenImage;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        screenImage = GetComponent<Image>();
        screenImage.color = new Color(0, 0, 0, 0); // Start fully transparent
    }

    /// <summary>
    /// Show a screen tint effect using the specified color, alpha, and fade speed. Called for damage, healing, etc.
    /// </summary>
    public void ShowEffect(Color effectColor, float initialAlpha = 0.5f, float fadeSpeed = 2f)
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeOutEffect(effectColor, initialAlpha, fadeSpeed));
    }

    private IEnumerator FadeOutEffect(Color color, float startAlpha, float fadeSpeed)
    {
        float alpha = startAlpha;
        color.a = alpha;
        screenImage.color = color;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            color.a = Mathf.Clamp01(alpha);
            screenImage.color = color;
            yield return null;
        }

        screenImage.color = new Color(0, 0, 0, 0);
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// Fades the screen to semi-transparent black for downed state.
    /// </summary>
    public void ShowDownedEffect()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeToSemiBlackEffect());
    }

    private IEnumerator FadeToSemiBlackEffect()
    {
        float alpha = screenImage.color.a;
        Color targetColor = new Color(0, 0, 0, 0.7f); // 70% black

        while (alpha < targetColor.a)
        {
            alpha += Time.deltaTime * 1f; // Speed of fade in
            alpha = Mathf.Clamp(alpha, 0, targetColor.a);
            screenImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        screenImage.color = targetColor;
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// Fades the screen back to clear after being revived.
    /// </summary>
    public void ShowReviveEffect()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeFromBlackEffect());
    }

    private IEnumerator FadeFromBlackEffect()
    {
        float alpha = screenImage.color.a;
        Color color = screenImage.color;

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 1.5f; // Speed of fade out
            alpha = Mathf.Clamp01(alpha);
            screenImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        screenImage.color = new Color(0, 0, 0, 0);
        currentFadeCoroutine = null;
    }
}
