using Unity.Netcode;
using UnityEngine;
using System;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;

public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public FixedString64Bytes DisplayName;
    public FixedString64Bytes SteamName;
    public int CoinsEarned; // ✅ Add this

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref DisplayName);
        serializer.SerializeValue(ref SteamName);
        serializer.SerializeValue(ref CoinsEarned); // ✅ Serialize this too
    }

    public bool Equals(LobbyPlayerData other)
    {
        return ClientId == other.ClientId && DisplayName.Equals(other.DisplayName);
    }
}

public class LobbyPlayerList : NetworkBehaviour
{
    public static LobbyPlayerList Instance;

    public NetworkList<LobbyPlayerData> Players = new NetworkList<LobbyPlayerData>();
    
    private void Start()
    {
        // When scene changes back to menu, force a refresh
        OnListChanged(default);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // persists across scenes
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        // 👇 Must be client + owner + Steam ready before calling RPC
        if (IsClient)
        {
            StartCoroutine(SendSteamNameNextFrame());
        }

        Players.OnListChanged += OnListChanged;
    }

    private System.Collections.IEnumerator SendSteamNameNextFrame()
    {
        yield return null; // wait 1 frame to ensure network is fully initialized

        if (SteamManager.Initialized)
        {
            string steamName = SteamFriends.GetPersonaName();
            SubmitSteamNameServerRpc(NetworkManager.Singleton.LocalClientId, steamName);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        Players.OnListChanged -= OnListChanged;
    }


    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.ShutdownInProgress)
            return;

        if (Players == null)
            return;

        for (int i = Players.Count - 1; i >= 0; i--)
        {
            if (Players[i].ClientId == clientId)
            {
                Players.RemoveAt(i);
            }
        }
    }



    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        Players.Add(new LobbyPlayerData
        {
            ClientId = clientId,
            DisplayName = $"Player {clientId}", // Optional, fallback
            SteamName = "" // Steam name will be filled in later
        });
    }

    private void OnListChanged(NetworkListEvent<LobbyPlayerData> change)
    {
        var playerList = new List<LobbyPlayerData>();
        foreach (var player in Players)
        {
            playerList.Add(player);
        }

        PlayerListUI.Instance?.RefreshList(playerList);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitSteamNameServerRpc(ulong clientId, string steamName)
    {
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].ClientId == clientId)
            {
                var updatedPlayer = Players[i];
                updatedPlayer.SteamName = steamName;
                Players[i] = updatedPlayer;

                Debug.Log($"✅ Updated Steam name for {clientId}: {steamName}");
                break;
            }
        }
    }
    
    public void ResetLobbyState()
    {
        Debug.Log("🔁 Resetting LobbyPlayerList...");

        // Detach event to prevent double subscription
        Players.OnListChanged -= OnListChanged;

        // Clear network player list
        Players.Clear();

        // Re-attach event
        Players.OnListChanged += OnListChanged;

        // 🔄 Clear the UI list too
        if (PlayerListUI.Instance != null)
        {
            PlayerListUI.Instance.ClearList();
        }
    }

}
