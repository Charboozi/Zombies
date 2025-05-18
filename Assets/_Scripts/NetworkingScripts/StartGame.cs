using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class StartGame : NetworkBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only host can start the game.");
            return;
        }

        GameOverManager.Instance?.ClearEliminationOrder();

        // ✅ Sync PvP flag to clients
        SyncPvPFlagClientRpc(GameModeManager.Instance.IsPvPMode);

        // ✅ Deduct wagers before switching scene
        DeductPvPWagers();

        // ✅ Load the game scene
        NetworkManager.Singleton.SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }

    [ClientRpc]
    private void SyncPvPFlagClientRpc(bool isPvP)
    {
        GameModeManager.Instance.SetPvPMode(isPvP);
        Debug.Log($"🛰 Synced PvP flag to client. PvP mode = {isPvP}");
    }

    public void DeductPvPWagers()
    {
        if (!IsServer || !GameModeManager.Instance.IsPvPMode)
            return;

        int wager = GameModeManager.Instance.PvPWagerAmount.Value;

        // Ask all clients to deduct from their CurrencyManager
        RequestClientWagerDeductionClientRpc(wager);

        GameModeManager.Instance.PvPRewardPot = wager * NetworkManager.Singleton.ConnectedClients.Count;
        Debug.Log($"💰 PvP Pot collected: {GameModeManager.Instance.PvPRewardPot}");
    }

    [ClientRpc]
    private void RequestClientWagerDeductionClientRpc(int wager)
    {
        if (IsHost) return; // Host already processes game logic

        if (CurrencyManager.Instance != null)
        {
            bool success = CurrencyManager.Instance.Spend(wager);

            if (!success)
            {
                Debug.LogError("🚫 Not enough coins to pay wager. Handle kick or block here.");
                // Optionally auto-leave or disable interaction
            }
        }
    }

}
