using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class FlashEffect : MonoBehaviour
{
    public static FlashEffect Instance;

    [SerializeField] private Volume volume;
    [SerializeField] private float flashIntensity = 2f;
    [SerializeField] private float flashDuration = 0.4f;

    private ColorAdjustments colorAdjustments;
    private float originalExposure;

    void Awake()
    {
        Instance = this;

        if (volume.profile.TryGet(out colorAdjustments))
        {
            originalExposure = colorAdjustments.postExposure.value;
        }
        else
        {
            Debug.LogError("⚠️ ColorAdjustments not found on Volume.");
        }
    }

    public void TriggerFlash()
    {
        if (colorAdjustments != null)
            StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        colorAdjustments.postExposure.value = flashIntensity;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            colorAdjustments.postExposure.value = Mathf.Lerp(flashIntensity, originalExposure, elapsed / flashDuration);
            yield return null;
        }

        colorAdjustments.postExposure.value = originalExposure;
    }
}
