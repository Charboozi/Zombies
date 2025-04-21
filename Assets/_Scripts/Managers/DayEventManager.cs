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
            AnnouncerVoiceManager.Instance.PlayVoiceLine("Power_Offline");
        });

        DayManager.Instance.ScheduleEventForDay(3, () =>
        {
            Debug.Log("🎉 Day 3 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.StartAlarmSequence();
            AnnouncerVoiceManager.Instance.PlayVoiceLine("Unknown_Entity");
        });

        DayManager.Instance.ScheduleEventForDay(8, () =>
        {
            Debug.Log("🎉 Day 8 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.StartAlarmSequence();
            AnnouncerVoiceManager.Instance.PlayVoiceLine("Unknown_Entity");
        });

        DayManager.Instance.ScheduleRecurringEvent(5, 5, day =>
        {
            Debug.Log($"⚡ Rampage Event Day {day}!");
            RampageManager.Instance.StartRampage();
            FogController.Instance?.FadeInFog();
            AlarmSequenceManager.Instance.StartAlarmSequence();
        });
        DayManager.Instance.ScheduleRecurringEvent(6, 5, day =>
        {
            Debug.Log($"⚡ End Rampage Event Day {day}!");
            RampageManager.Instance.EndRampage();
            FogController.Instance?.FadeOutFog();

        });

    }
}
