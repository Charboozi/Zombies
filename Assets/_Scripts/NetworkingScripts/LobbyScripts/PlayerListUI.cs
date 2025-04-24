using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerListUI : MonoBehaviour
{
    public static PlayerListUI Instance;

    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private Transform playerListParent;

    private void Awake()
    {
        Instance = this;
    }

    public void RefreshList(List<LobbyPlayerData> players)
    {
        foreach (Transform child in playerListParent)
            Destroy(child.gameObject);

        foreach (var player in players)
        {
            var entry = Instantiate(playerEntryPrefab, playerListParent);
            var text = entry.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = player.DisplayName.ToString();
        }
    }

    public void ClearList()
    {
        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject);
        }
    }
}
