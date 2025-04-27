using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameOverManager : NetworkBehaviour
{
    public static GameOverManager Instance;

    private readonly List<EntityHealth> playerEntities = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(EntityHealth player)
    {
        if (!IsServer || player == null || !player.CompareTag("Player")) return;

        if (!playerEntities.Contains(player))
        {
            playerEntities.Add(player);
            player.OnTakeDamage += () => StartCoroutine(DelayedGameOverCheck());
        }
    }

    public void UnregisterPlayer(EntityHealth player)
    {
        if (!IsServer || player == null || !player.CompareTag("Player")) return;

        if (playerEntities.Contains(player))
        {
            playerEntities.Remove(player);
            player.OnTakeDamage -= () => CheckGameOverCondition(player);
        }
    }

    private void CheckGameOverCondition(EntityHealth _)
    {
        if (!IsServer) return;

        foreach (var player in playerEntities)
        {
            if (player == null)
            {
                Debug.LogWarning("❗ A registered player is null.");
                continue;
            }

            if (!player.isDowned.Value)
            {
                return;
            }
        }

        Debug.Log("✅ All players are downed. Triggering game over...");
        TriggerGameOver();
    }


    private void TriggerGameOver()
    {
        if (NetworkManager.Singleton.SceneManager != null)
        {
            // 🛠 Safely loop by creating a copy first
            var playersToDespawn = new List<EntityHealth>(playerEntities);

            foreach (var player in playersToDespawn)
            {
                if (player != null && player.TryGetComponent(out NetworkObject netObj) && netObj.IsSpawned)
                {
                    netObj.Despawn(true);
                }
            }

            NetworkManager.Singleton.SceneManager.LoadScene("GameOver", LoadSceneMode.Single);
            StartCoroutine(ShutdownAfterDelay(10f));
        }
        else
        {
            Debug.LogWarning("NetworkSceneManager not set up correctly!");
        }
    }


    private IEnumerator ShutdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            NetworkManager.Singleton.DisconnectClient(client.Key);
        }

        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu"); // optional fallback
    }

    private IEnumerator DelayedGameOverCheck()
    {
        yield return new WaitForSeconds(0.1f); // slight delay to allow state update
        CheckGameOverCondition(null);
    }
}
