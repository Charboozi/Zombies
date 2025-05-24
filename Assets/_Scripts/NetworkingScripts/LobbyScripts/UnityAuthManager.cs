using UnityEngine;
using TMPro;
using Steamworks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;


public class UnityAuthManager : MonoBehaviour
{
    public static UnityAuthManager Instance;

    [SerializeField] private TMP_Text playerIdText;

    public bool IsAuthenticated => SteamClient.IsValid;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //await AuthenticateUGS();

        // Steam overlay name (visual only)
        if (SteamClient.IsValid && playerIdText != null)
        {
            playerIdText.text = SteamClient.Name;
        }
    }

    public async Task WaitUntilAuthenticated()
    {
        int timeoutMs = 5000;
        int elapsed = 0;
        while (!IsAuthenticated && elapsed < timeoutMs)
        {
            await Task.Delay(100);
            elapsed += 100;
        }

        if (!IsAuthenticated)
            Debug.LogWarning("❗ Timeout waiting for Unity Authentication.");
    }
}
