using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private Button startButton;
    [SerializeField] private GameObject playerPrefab; // Drag your player prefab here

    void Start()
    {
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(ChangeScene);
    }

    private void ChangeScene()
    {
        if (NetworkManager.Singleton.IsServer) // Only the server/host can change scenes
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("Only the host can start the game.");
        }
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == "GameScene" && NetworkManager.Singleton.IsServer)
        {
            // Spawn players for all connected clients
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                Vector3 spawnPosition = GetSpawnPosition(client.ClientId);
                GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
            }

            // Optional: Unsubscribe from the event to avoid duplicate calls
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
        }
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // TODO: Customize spawn position per client if needed
        return new Vector3(clientId * -2f, 2f, 5f); // Simple example: spread out players
    }
}
