using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class AlarmSequenceManager : NetworkBehaviour
{
    public static AlarmSequenceManager Instance { get; private set; }

    [Header("Lights to Flicker (will toggle GameObject.active)")]
    [SerializeField] private Light[] lights;

    [Header("Audio")]
    [SerializeField] private AudioSource flickerAudioSource;

    [Header("Sequence Settings")]
    [SerializeField] private float sequenceDuration = 20f;
    [SerializeField] private float flickerInterval  = 0.1f;

    [Header("Rotation Settings")]
    [Tooltip("Degrees per second to spin each light on Y during the alarm.")]
    [SerializeField] private float rotationSpeedY = 45f;

    private GameObject[] lightObjects;
    private Coroutine    activeSequence;
    private bool         sequenceActive = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache the light GameObjects
        lightObjects = lights
            .Where(l => l != null)
            .Select(l => l.gameObject)
            .ToArray();
    }

    private void Update()
    {
        // Smoothly rotate all light GameObjects while the sequence is active
        if (sequenceActive)
        {
            float deltaY = rotationSpeedY * Time.deltaTime;
            foreach (var go in lightObjects)
            {
                go.transform.Rotate(0f, deltaY, 0f, Space.World);
            }
        }
    }

    /// <summary>
    /// Call this on the server to start the alarm sequence for everyone.
    /// </summary>
    public void StartAlarmSequence()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Only the server can start the alarm sequence!");
            return;
        }

        // Tell all clients to start
        StartAlarmSequenceClientRpc();

        // Start it on the server as well
        RestartSequence();
    }

    [ClientRpc]
    private void StartAlarmSequenceClientRpc()
    {
        // Server already started it locally
        if (IsServer) return;

        RestartSequence();
    }

    private void RestartSequence()
    {
        // Stop any existing routine
        if (activeSequence != null)
            StopCoroutine(activeSequence);

        // Begin new flicker & rotation coroutine
        activeSequence    = StartCoroutine(FlickerAndRotateRoutine());
        sequenceActive    = true;
    }

    private IEnumerator FlickerAndRotateRoutine()
    {
        float elapsed   = 0f;
        bool  lightsOn  = false;

        // Play audio once
        if (flickerAudioSource != null && !flickerAudioSource.isPlaying)
            flickerAudioSource.Play();

        // Loop toggling on/off at the given interval
        while (elapsed < sequenceDuration)
        {
            lightsOn = !lightsOn;
            foreach (var go in lightObjects)
                go.SetActive(lightsOn);

            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        // Ensure everything is off at the end
        foreach (var go in lightObjects)
            go.SetActive(false);

        if (flickerAudioSource != null && flickerAudioSource.isPlaying)
            flickerAudioSource.Stop();

        // Tear down
        sequenceActive = false;
        activeSequence = null;
    }
}
