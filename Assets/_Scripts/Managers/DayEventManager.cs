using UnityEngine;

public class DayEventManager : MonoBehaviour
{
    private void Start()
    {
        // Wait until DayManager is ready
        if (DayManager.Instance == null)
        {
            Debug.LogWarning("DayManager not found.");
            return;
        }

        // 🔹 One-time event on Day 5
        DayManager.Instance.ScheduleEventForDay(2, () =>
        {
            Debug.Log("🎉 Day 3 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.StartAlarmSequence();
        });

        // 🔹 Recurring every 3 days starting Day 2
        DayManager.Instance.ScheduleRecurringEvent(2, 3, day =>
        {
            Debug.Log($"⚡ Recurring Event on Day {day}");
        });

        // 🔹 Day 10 unlock feature
        DayManager.Instance.ScheduleEventForDay(10, () =>
        {
            Debug.Log("🔓 Unlocking turret feature!");
        });
    }
}
