using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Steamworks;
using System;
using System.Threading.Tasks;

public class SteamOverlayRelayJoinHandler : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel; // 👈 Set this in Inspector
    [SerializeField] private GameObject lobbyPanel;    // 👈 Set this in Inspector

    [Header("Debug Text (optional)")]
    [SerializeField] private TMP_Text statusText;

    private Callback<LobbyEnter_t> lobbyEnterCallback;

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("❌ Steam not initialized.");
            return;
        }

        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnteredFromSteamOverlay);
        Debug.Log("📡 Steam overlay join handler ready.");
    }

private async void OnLobbyEnteredFromSteamOverlay(LobbyEnter_t callback)
{
    CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
    Debug.Log($"✅ Joined Steam lobby: {lobbyID}");

    // Retry to get join code
    string joinCode = null;
    float timeout = 5f;
    float elapsed = 0f;

    while (string.IsNullOrEmpty(joinCode) && elapsed < timeout)
    {
        joinCode = SteamMatchmaking.GetLobbyData(lobbyID, "joinCode");
        await Task.Delay(250);
        elapsed += 0.25f;
    }

    if (string.IsNullOrEmpty(joinCode))
    {
        Debug.LogError("❌ Still no joinCode after retry.");
        return;
    }

    Debug.Log($"🔗 Found joinCode: {joinCode}");

    // Auth
    await UnityServices.InitializeAsync();
    if (!AuthenticationService.Instance.IsSignedIn)
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    // Join relay
    var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
    var relayServerData = AllocationUtils.ToRelayServerData(joinAllocation, "dtls");

    var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
    transport.SetRelayServerData(relayServerData);

    // Start client
    NetworkManager.Singleton.StartClient();

    // Add connection logs
    NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
    {
        Debug.Log($"✅ Connected to host as client! ClientId: {id}");
    };

    NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
    {
        Debug.LogWarning($"❌ Client {id} disconnected.");
    };

    // Show UI
    if (lobbyPanel != null) lobbyPanel.SetActive(true);
    if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
}


    private void SetStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
            statusText.text = message;
    }
}
