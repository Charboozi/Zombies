using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    [Header("Steam Settings")]
    [Tooltip("480 = Spacewar (test AppID). Replace with your real Steam App ID for release.")]
    public uint steamAppId = 480;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Steam
        try
        {
            if (!SteamClient.IsValid)
            {
                SteamClient.Init(steamAppId, true);
                Debug.Log("✅ SteamClient initialized (Facepunch)");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Steam initialization failed: " + ex.Message);
        }
    }

    private void Update()
    {
        if (SteamClient.IsValid)
        {
            SteamClient.RunCallbacks();
        }
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("🧪 Trying to open Steam overlay: friends");
            SteamFriends.OpenOverlay("friends");
        }
    }

    private void OnApplicationQuit()
    {
        if (SteamClient.IsValid)
        {
            SteamClient.Shutdown();
            Debug.Log("🛑 SteamClient shutdown");
        }
    }
}
