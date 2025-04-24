using Unity.Netcode;
using UnityEngine;
using System;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public struct LobbyPlayerData : INetworkSerializable, IEquatable<LobbyPlayerData>
{
    public ulong ClientId;
    public FixedString64Bytes DisplayName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref DisplayName);
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

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        Players.OnListChanged += OnListChanged;
    }

    public override void OnNetworkDespawn()
    {
        Players.Clear();
    }

    private void OnClientDisconnected(ulong clientId)
    {
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

        // You can replace this with a proper name from AuthService or input
        var playerName = $"Player {clientId}";

        Players.Add(new LobbyPlayerData
        {
            ClientId = clientId,
            DisplayName = playerName
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
    
}
