using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip[] soundClips;   // List of sounds to play
    public float minInterval = 3f;   // Minimum time between sounds
    public float maxInterval = 8f;   // Maximum time between sounds

    [Header("Pitch Variation")]
    public float minPitch = 0.9f;    // Minimum random pitch
    public float maxPitch = 1.1f;    // Maximum random pitch

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
            PlayRandomSound();
        }
    }

    private void PlayRandomSound()
    {
        if (soundClips.Length == 0 || audioSource == null)
        {
            Debug.LogWarning("No sound clips assigned or missing AudioSource!");
            return;
        }

        AudioClip randomClip = soundClips[Random.Range(0, soundClips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(randomClip);
    }

    public void StopSounds()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null; // ✅ Safe cleanup
        }

        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}
