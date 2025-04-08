using UnityEngine;
using System;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Day Settings")]
    [Tooltip("Length of a full day in seconds, Example: 2 minutes = 1 day")]
    [SerializeField] private float dayLengthInSeconds = 120f;

    public float CurrentTime { get; private set; } // Current time in seconds
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
        CurrentTime += Time.deltaTime;

        int day = Mathf.FloorToInt(CurrentDay);
        if (day != lastDayChecked)
        {
            lastDayChecked = day;
            OnNewDayStarted?.Invoke(day);
            Debug.Log($"🌞 New day: {day}");
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
