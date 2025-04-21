using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour
{
    public static FogController Instance { get; private set; }

    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float targetFogDensity = 0.1f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RenderSettings.fog = true; // Keep always enabled
        RenderSettings.fogDensity = 0f;
    }

    public void FadeInFog()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeFog(RenderSettings.fogDensity, targetFogDensity));
    }

    public void FadeOutFog()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeFog(RenderSettings.fogDensity, 0f));
    }

    private IEnumerator FadeFog(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            RenderSettings.fogDensity = Mathf.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        RenderSettings.fogDensity = end;
        fadeRoutine = null;
    }
}
