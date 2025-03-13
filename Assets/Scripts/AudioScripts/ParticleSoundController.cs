using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem), typeof(AudioSource))]
public class ParticleSoundController : MonoBehaviour
{
    private ParticleSystem ps;
    private AudioSource audioSource;
    private int lastParticleCount = 0;
    private bool isLooping = false;
    private bool isPlaying = false; 

    private float fadeInSpeed = 2f;  // Speed of fade in
    private float fadeOutSpeed = 2f; // Speed of fade out
    private Coroutine fadeCoroutine;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            isLooping = audioSource.loop; // Check if the sound is looping
        }
    }

    void Update()
    {
        if (ps.isEmitting)
        {
            if (isLooping)
            {
                // Looping sounds (flamethrowers, lasers)
                if (!isPlaying)
                {
                    isPlaying = true;
                    StartFadeIn();
                }
            }
            else
            {
                // Single-shot sounds (bullets, explosions)
                if (ps.particleCount > lastParticleCount)
                {
                    PlayGunshotSound();
                }
            }
        }
        else
        {
            // Stop looping sounds when no particles
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
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    private void StartFadeIn()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeAudio(1f, fadeInSpeed)); // Fade to full volume
    }

    private void StartFadeOut()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeAudio(0f, fadeOutSpeed)); // Fade to zero volume
    }

    private IEnumerator FadeAudio(float targetVolume, float speed)
    {
        if (targetVolume > 0f && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * speed;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume == 0f)
        {
            audioSource.Stop();
        }
    }
}
