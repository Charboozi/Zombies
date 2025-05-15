using UnityEngine;
using TMPro;
using System.Collections;

public class FeedEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float fadeTime = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(lifetime);

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }

        Destroy(gameObject);
    }
}
