using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Steamworks;
using Steamworks.Data;
using System;

[RequireComponent(typeof(Button))]
public class SteamHostLobbyButton : MonoBehaviour
{
    public enum GameModeType { PvP, CoOp }

    [SerializeField] private GameModeType selectedGameMode; // Set this via Inspector or another button
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Button createButton;

    private Lobby currentLobby;

    public async void CreateLobby()
    {
        createButton.interactable = false;

        try
        {
            var result = await SteamMatchmaking.CreateLobbyAsync(5);
            if (result.HasValue)
            {
                currentLobby = result.Value;
                currentLobby.SetJoinable(true);

                // Set metadata
                currentLobby.SetData("hostId", SteamClient.SteamId.ToString());
                currentLobby.SetData("gameMode", selectedGameMode.ToString()); // 👈 Store PvP or CoOp

                if (codeDisplay != null)
                    codeDisplay.text = SteamClient.Name;

                NetworkManager.Singleton.StartHost();
                Debug.Log($"✅ Hosting Steam lobby. HostID: {SteamClient.SteamId}, Mode: {selectedGameMode}");

                SteamFriends.OpenGameInviteOverlay(currentLobby.Id);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Failed to create Steam lobby: " + ex.Message);
            createButton.interactable = true;
        }
    }

}
