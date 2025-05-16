using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PlayerListUI : MonoBehaviour
{
    public static PlayerListUI Instance;

    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private Transform playerListParent;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        // Wait a short delay to ensure LobbyPlayerList.Instance is ready
        Invoke(nameof(SafeRefresh), 0.1f);
    }

    private void SafeRefresh()
    {
        if (LobbyPlayerList.Instance != null)
        {
            var players = new List<LobbyPlayerData>();

            foreach (var player in LobbyPlayerList.Instance.Players)
            {
                players.Add(player);
            }

            RefreshList(players);
        }
    }

    public void RefreshList(List<LobbyPlayerData> players)
    {
        if (playerListParent == null) return; // ✅ Safety check

        foreach (Transform child in playerListParent)
        {
            if (child != null)
                Destroy(child.gameObject);
        }

        foreach (var player in players)
        {
            if (playerListParent == null) return;

            var entry = Instantiate(playerEntryPrefab, playerListParent);

            var nameText = entry.transform.Find("NameText")?.GetComponent<TMP_Text>();
            var rewardText = entry.transform.Find("RewardText")?.GetComponent<TMP_Text>();

            if (nameText != null)
                nameText.text = !string.IsNullOrEmpty(player.SteamName.ToString()) ? player.SteamName.ToString() : player.DisplayName.ToString();

            if (rewardText != null)
                rewardText.text = player.CoinsEarned > 0 ? $"$+{player.CoinsEarned}" : "";
        }
    }

    public void ClearList()
    {
        if (playerListParent == null) return; // ✅ Safety check

        foreach (Transform child in playerListParent)
        {
            if (child != null)
                Destroy(child.gameObject);
        }
    }
}