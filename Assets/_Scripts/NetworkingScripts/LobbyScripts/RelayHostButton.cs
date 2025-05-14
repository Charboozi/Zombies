using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Steamworks;

[RequireComponent(typeof(Button))]
public class RelayHostButton : MonoBehaviour
{
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private CSteamID currentLobbyID;
    private Callback<LobbyCreated_t> lobbyCreated;

    private void Start()
    {
        //createButton.onClick.AddListener(CreateRelayHost);

        if (!SteamManager.Initialized) return;

        if (!SteamManager.Initialized) return;
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
    }

    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 5);
    }

    private async void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("❌ Steam Lobby creation failed.");
            return;
        }

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log("✅ Steam Lobby created!");

        // Step 1: Create Unity Relay Allocation
        var allocation = await RelayService.Instance.CreateAllocationAsync(5);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.Log($"🔗 Relay join code: {joinCode}");

        // Step 2: Save to lobby metadata
        SteamMatchmaking.SetLobbyData(currentLobbyID, "joinCode", joinCode);
        SteamMatchmaking.SetLobbyData(currentLobbyID, "host", SteamUser.GetSteamID().ToString());

        // Step 3: Start host with NGO
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);
        NetworkManager.Singleton.StartHost();

        // (Optional) Open invite overlay
        SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
        Debug.Log("Opening invite overlay...");
    }

    private async void CreateRelayHost()
    {
        createButton.interactable = false;
        joinButton.interactable = false; 

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            if (codeDisplay != null)
                codeDisplay.text = $"{joinCode}";

            Debug.Log("Relay host started with code: " + joinCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("Host creation failed: " + ex.Message);
            createButton.interactable = true;
            joinButton.interactable = true;  // 🔁 Re-enable if something failed
        }
    }
}
