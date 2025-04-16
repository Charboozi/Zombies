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

        DayManager.Instance.ScheduleEventForDayTime(1, 3f, () =>
        {
            Debug.Log("🚨 3 seconds into Day 3: Triggering Start Sequence!");
            LightmapSwitcher.Instance.RequestBlackout();
            
        });

        DayManager.Instance.ScheduleEventForDay(3, () =>
        {
            Debug.Log("🎉 Day 3 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.StartAlarmSequence();
            AnnouncerVoiceManager.Instance.PlayVoiceLine("Unknown_Enitite");
        });

        DayManager.Instance.ScheduleRecurringEvent(2, 3, day =>
        {
            Debug.Log($"⚡ Recurring Event on Day {day}");
        });

        DayManager.Instance.ScheduleEventForDay(10, () =>
        {
            Debug.Log("🔓 Unlocking turret feature!");
        });
    }
}
