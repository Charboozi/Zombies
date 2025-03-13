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

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(SoundRoutine());
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
        audioSource.pitch = Random.Range(minPitch, maxPitch); // Apply random pitch
        audioSource.PlayOneShot(randomClip);
    }
}
