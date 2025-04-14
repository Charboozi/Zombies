using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class AlarmSequenceManager : NetworkBehaviour
{
    public static AlarmSequenceManager Instance { get; private set; }

    [Header("Lights to Flicker")]
    [SerializeField] private Light[] lights;

    [Header("Audio")]
    [SerializeField] private AudioSource flickerAudioSource;

    [Header("Sequence Settings")]
    [SerializeField] private float sequenceDuration = 20f;
    [SerializeField] private float flickerInterval = 0.1f;

    private Coroutine activeSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            StartAlarmSequence();
        }
    }

    // ✅ You call this from the server to trigger for everyone!
    public void StartAlarmSequence()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can start the alarm sequence!");
            return;
        }

        // Tell all clients to start
        StartAlarmSequenceClientRpc();

        // Optional: Start on server too (in case you need logic here)
        if (activeSequence != null)
        {
            StopCoroutine(activeSequence);
        }
        activeSequence = StartCoroutine(FlickerRoutine());
    }

    [ClientRpc]
    private void StartAlarmSequenceClientRpc()
    {
        // Skip server, we already started locally.
        if (IsServer) return;

        if (activeSequence != null)
        {
            StopCoroutine(activeSequence);
        }
        activeSequence = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        float elapsed = 0f;
        bool lightsOn = false;

        if (flickerAudioSource != null && !flickerAudioSource.isPlaying)
        {
            flickerAudioSource.Play();
        }

        while (elapsed < sequenceDuration)
        {
            lightsOn = !lightsOn; // Toggle the lights

            foreach (var light in lights)
            {
                if (light != null)
                    light.enabled = lightsOn;
            }

            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        // ✅ At the end, make sure ALL lights are OFF
        foreach (var light in lights)
        {
            if (light != null)
                light.enabled = false;
        }

        if (flickerAudioSource != null && flickerAudioSource.isPlaying)
        {
            flickerAudioSource.Stop();
        }

        activeSequence = null;
    }

}
