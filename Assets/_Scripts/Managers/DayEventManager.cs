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

        DayManager.Instance.ScheduleEventForDayTime(1, 7f, () =>
        {
            Debug.Log("🚨 7 seconds into Day 1: Triggering Start Sequence!");
            LightmapSwitcher.Instance.RequestBlackout();
            AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Power_Offline");
        });

        DayManager.Instance.ScheduleEventForDay(3, () =>
        {
            Debug.Log("🎉 Day 3 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.ActivateAlarm();
            AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Unknown_Entity");
            MusicPlayer.Instance.ServerPlayMusic("TunnelDwellerSpawn");
        });

        DayManager.Instance.ScheduleEventForDay(8, () =>
        {
            Debug.Log("🎉 Day 8 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.ActivateAlarm();
            AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Unknown_Entity");
            MusicPlayer.Instance.ServerPlayMusic("SnatcherSpawn");
        });

        DayManager.Instance.ScheduleRecurringEvent(5, 20, day =>
        {
            Debug.Log($"⚡ Rampage Event Day {day}!");
            RampageManager.Instance.StartRampage();
            FogController.Instance?.FadeInFog();
            AlarmSequenceManager.Instance.ActivateAlarm();
            MusicPlayer.Instance.ServerPlayMusic("DeathHunterLite");
        });
        DayManager.Instance.ScheduleRecurringEvent(10, 20, day =>
        {
            Debug.Log($"⚡ Rampage Event Day {day}!");
            RampageManager.Instance.StartRampage();
            FogController.Instance?.FadeInFog();
            AlarmSequenceManager.Instance.ActivateAlarm();
            MusicPlayer.Instance.ServerPlayMusic("DeathHunterMaster");
        });

        DayManager.Instance.ScheduleRecurringEvent(6, 5, day =>
        {
            Debug.Log($"⚡ End Rampage Event Day {day}!");
            RampageManager.Instance.EndRampage();
            FogController.Instance?.FadeOutFog();

        });

    }
}
