using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class ParticleSoundController : MonoBehaviour
{
    private ParticleSystem ps;
    private AudioSource audioSource;
    private int lastParticleCount = 0;

    [Header("Settings")]
    [SerializeField] private bool isLooping = false;
    [SerializeField] private float fadeInSpeed = 2f;
    [SerializeField] private float fadeOutSpeed = 2f;

    private Coroutine fadeCoroutine;
    private bool isPlaying = false;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (ps.isEmitting)
        {
            if (isLooping)
            {
                if (!isPlaying)
                {
                    isPlaying = true;
                    StartFadeIn();
                }
            }
            else
            {
                if (ps.particleCount > lastParticleCount)
                {
                    PlayGunshotSound();
                }
            }
        }
        else
        {
            if (isLooping && isPlaying)
            {
                isPlaying = false;
                StartFadeOut();
            }
        }

        lastParticleCount = ps.particleCount;
    }

    private void PlayGunshotSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            AudioSource.PlayClipAtPoint(audioSource.clip, transform.position);
        }
    }

    private void StartFadeIn()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(1f, fadeInSpeed));
    }

    private void StartFadeOut()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeAudio(0f, fadeOutSpeed));
    }

    private IEnumerator FadeAudio(float targetVolume, float speed)
    {
        if (targetVolume > 0f && !audioSource.isPlaying)
        {
            audioSource.volume = 0f;
            audioSource.Play();
        }

        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume == 0f)
        {
            audioSource.Stop();
        }
    }

    private void OnDisable()
    {
        if (isLooping && isPlaying)
        {
            isPlaying = false;
            audioSource.Stop(); // Stop loop immediately when object is disabled
        }
    }
}
