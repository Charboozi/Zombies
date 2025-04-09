using UnityEngine;
using Unity.Netcode;
using System;

public class DayManager : NetworkBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Settings")]
    [Tooltip("Length of a full day in seconds, Example: 2 minutes = 1 day")]
    [SerializeField] private float dayLengthInSeconds = 120f;

    private NetworkVariable<float> currentTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public float CurrentTime => currentTime.Value; // Read-only for clients
    public float CurrentDay => CurrentTime / dayLengthInSeconds;
    public int CurrentDayInt => Mathf.FloorToInt(CurrentDay);

    public event Action<int> OnNewDayStarted;

    private int lastDayChecked = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (!IsServer) return; // Only the server updates time

        currentTime.Value += Time.deltaTime;

        int day = Mathf.FloorToInt(CurrentDay);
        if (day != lastDayChecked)
        {
            lastDayChecked = day;
            OnNewDayStarted?.Invoke(day);
            Debug.Log($"🌞 New day: {day}");
        }
    }

    private void OnEnable()
    {
        currentTime.OnValueChanged += OnTimeChanged;
    }

    private void OnDisable()
    {
        currentTime.OnValueChanged -= OnTimeChanged;
    }

    private void OnTimeChanged(float oldValue, float newValue)
    {
        int day = Mathf.FloorToInt(newValue / dayLengthInSeconds);
        if (day != lastDayChecked)
        {
            lastDayChecked = day;
            OnNewDayStarted?.Invoke(day);
            Debug.Log($"🌞 (Client) New day: {day}");
        }
    }

    public float GetDayFraction(float days)
    {
        return days * dayLengthInSeconds;
    }

    public bool HasReachedDay(float targetDay)
    {
        return CurrentDay >= targetDay;
    }
}
