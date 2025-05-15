using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip[] soundClips;
    public float minInterval = 3f;
    public float maxInterval = 8f;

    [Header("Pitch Variation")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    [Header("Voice Limiting")]
    public static int currentVoices = 0;
    public static int maxVoices = 10; // 👈 You can tweak this globally

    private AudioSource audioSource;
    private Coroutine playRoutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playRoutine = StartCoroutine(SoundRoutine());
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);

            if (currentVoices < maxVoices)
            {
                PlayRandomSound();
            }
        }
    }

    private void PlayRandomSound()
    {
        if (soundClips.Length == 0 || audioSource == null)
            return;

        AudioClip randomClip = soundClips[Random.Range(0, soundClips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.PlayOneShot(randomClip);
        StartCoroutine(TrackVoiceDuration(randomClip.length));
    }

    private IEnumerator TrackVoiceDuration(float duration)
    {
        currentVoices++;
        yield return new WaitForSeconds(duration);
        currentVoices--;
    }

    public void StopSounds()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}
