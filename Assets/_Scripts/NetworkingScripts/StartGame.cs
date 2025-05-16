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

        // ✅ Sync PvP flag to clients before loading scene
        SyncPvPFlagClientRpc(GameModeManager.Instance.IsPvPMode);

        // ✅ Start the scene
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
}
