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

        DayManager.Instance.ScheduleEventForDayTime(1, 10f, () =>
        {
            Debug.Log("🚨 10 seconds into Day 1: Triggering Start Sequence!");
            
            if (!GameModeManager.Instance.IsPvPMode)
            {
                LightmapSwitcher.Instance.RequestBlackout();
            }
        });

        DayManager.Instance.ScheduleEventForDay(3, () =>
        {
            Debug.Log("🎉 Day 3 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.ActivateAlarm();
            AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Unknown_Entity");
            MusicPlayer.Instance.ServerPlayMusic("TunnelDwellerSpawn");
            GameFeedManager.Instance?.PostFeedMessageServerRpc("an Unknown entity has entered the station!");
        });

        DayManager.Instance.ScheduleEventForDay(8, () =>
        {
            Debug.Log("🎉 Day 8 Event Triggered!, Unknown entitie has entered the area, going in to lockdown mode");
            AlarmSequenceManager.Instance.ActivateAlarm();
            AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Unknown_Entity");
            MusicPlayer.Instance.ServerPlayMusic("SnatcherSpawn");
            GameFeedManager.Instance?.PostFeedMessageServerRpc("an Unknown entity has entered the station!");
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
