using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Show a fade scren effect. e.g healing it shows green and then fades
/// </summary>
public class FadeScreenEffect : MonoBehaviour
{
    public static FadeScreenEffect Instance;

    private Image screenImage;
    private Coroutine currentFadeCoroutine;

    void Awake()
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
    /// Show a screen tint effect using the specified color, alpha, and fade speed. Called when need effect
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
            color.a = alpha;
            screenImage.color = color;
            yield return null;
        }

        screenImage.color = new Color(0, 0, 0, 0);
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// Fades the screen to black permanently (used for death TEMPORARY BEFORE DOWNED EFFECT, WILL GET REMOVED).
    /// </summary>
    public void ShowDeathEffect()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        currentFadeCoroutine = StartCoroutine(FadeToBlackEffect());
    }

    private IEnumerator FadeToBlackEffect()
    {
        float alpha = 0f;
        Color blackColor = new Color(0, 0, 0, 0);
        screenImage.color = blackColor;

        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 0.5f;
            screenImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        screenImage.color = new Color(0, 0, 0, 1f);
    }
}
