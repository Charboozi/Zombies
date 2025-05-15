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
        if (!IsServer) return;

        if (NetworkManager.Singleton.SceneManager != null)
        {
            Debug.Log("✅ Triggering game over visuals...");
            RewardAllPlayersBasedOnDay();
            ShowGameOverEffectClientRpc();
            SetCanInteractClientRpc(true);
            StartCoroutine(DelayedGameOverSceneLoad(4f)); // ⏱ Wait 4s for screen fade
        }
        else
        {
            Debug.LogWarning("NetworkSceneManager not set up correctly!");
        }
    }

    private IEnumerator DelayedGameOverSceneLoad(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Optional: Despawn all players cleanly
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

    [ClientRpc]
    private void ShowGameOverEffectClientRpc()
    {
        if (FadeScreenEffect.Instance != null)
        {
            FadeScreenEffect.Instance.ShowDeathEffect(3.7f); // Fade to deep red in 4 seconds
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

    private void RewardAllPlayersBasedOnDay()
    {
        int day = DayManager.Instance.CurrentDayInt;

        // 🔢 Base reward and growth multiplier
        float baseReward = 10f;         // starting coins at day 0
        float growthMultiplier = 1.25f; // increase rate (adjust as needed)

        int reward = Mathf.RoundToInt(baseReward * Mathf.Pow(growthMultiplier, day));

        Debug.Log($"🎁 Rewarding all players {reward} coins for reaching Day {day}");

        RewardClientsClientRpc(reward);
    }

    [ClientRpc]
    private void RewardClientsClientRpc(int reward)
    {
        if (CurrencyManager.Instance != null)
        {
            Debug.Log($"🎉 Received {reward} coins!");
            CurrencyManager.Instance.Add(reward);
        }
    }
    
    [ClientRpc]
    private void SetCanInteractClientRpc(bool value)
    {
        PlayerInput.CanInteract = value;
    }
    
}
