using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageScreenEffect : MonoBehaviour
{
    public static DamageScreenEffect Instance; // Singleton for easy access
    private Image damageImage;
    private float fadeSpeed = 2f; // Speed of fade-out effect
    private Color damageColor = new Color(1, 0, 0, 0.5f); // Red with 50% opacity

    void Awake()
    {
        Instance = this;
        damageImage = GetComponent<Image>();
        damageImage.color = new Color(1, 0, 0, 0); // Start fully transparent
    }

    public void ShowDamageEffect()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutEffect());
    }
    private IEnumerator FadeOutEffect()
    {
        damageImage.color = damageColor; // Make screen red

        float alpha = damageColor.a;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            damageImage.color = new Color(1, 0, 0, alpha);
            yield return null;
        }
    }

    public void ShowDeathEffect()
    {
        StopAllCoroutines(); // Stop any previous fade effects
        StartCoroutine(FadeToBlackEffect());
    }

    private IEnumerator FadeToBlackEffect()
    {
        float alpha = 0f;
        Color blackColor = new Color(0, 0, 0, 0); // Start fully transparent
        damageImage.color = blackColor;

        while (alpha < 1f) // Gradually increase opacity
        {
            alpha += Time.deltaTime * 0.5f; // Adjust fade speed here
            damageImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure it's fully black at the end
        damageImage.color = new Color(0, 0, 0, 1);
    }

}
