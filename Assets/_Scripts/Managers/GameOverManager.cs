using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameOverManager : NetworkBehaviour
{
    public static GameOverManager Instance;

    private readonly List<EntityHealth> playerEntities = new();

    private ulong lastSurvivorClientId;
    private ulong currentlyLastAliveClientId;

    private readonly List<ulong> eliminationOrder = new(); // From first dead to last

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
            player.OnDowned += OnPlayerDowned;
        }
    }

    public void UnregisterPlayer(EntityHealth player)
    {
        if (!IsServer || player == null || !player.CompareTag("Player")) return;

        if (playerEntities.Contains(player))
        {
            playerEntities.Remove(player);
            player.OnTakeDamage -= () => CheckGameOverCondition(player);
            player.OnDowned -= OnPlayerDowned;
        }
    }

    private void OnPlayerDowned(EntityHealth player)
    {
        if (!IsServer || player == null) return;

        ulong clientId = player.OwnerClientId;
        if (!eliminationOrder.Contains(clientId))
        {
            eliminationOrder.Add(clientId);
            Debug.Log($"☠️ Player {clientId} downed. Elimination order: {eliminationOrder.Count}");
        }
    }

    public void ClearEliminationOrder()
    {
        eliminationOrder.Clear();
    }

    private void CheckGameOverCondition(EntityHealth _)
    {
        if (!IsServer) return;

        ulong newLastAlive = 0;
        int aliveCount = 0;

        foreach (var player in playerEntities)
        {
            if (player == null) continue;

            Debug.Log($"🔍 Player {player.OwnerClientId} isDowned: {player.isDowned.Value}");

            if (!player.isDowned.Value)
            {
                aliveCount++;
                newLastAlive = player.OwnerClientId;
            }
        }

        // Track the last standing player when only one is alive
        if (aliveCount == 1)
        {
            currentlyLastAliveClientId = newLastAlive;
        }

        // If someone is still alive, don't trigger game over yet
        if (aliveCount > 0)
            return;

        // All players are down — use tracked last survivor
        lastSurvivorClientId = currentlyLastAliveClientId;
        Debug.Log($"✅ All players are down. Last survivor was Client {lastSurvivorClientId}");

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
            StartCoroutine(DelayedGameOverSceneLoad(4f)); // Fade duration
        }
        else
        {
            Debug.LogWarning("⚠️ NetworkSceneManager not found.");
        }
    }

    private IEnumerator DelayedGameOverSceneLoad(float delay)
    {
        yield return new WaitForSeconds(delay);

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

    private IEnumerator ShutdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            NetworkManager.Singleton.DisconnectClient(client.Key);
        }

        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator DelayedGameOverCheck()
    {
        yield return new WaitForSeconds(0.1f); // Allow state updates
        CheckGameOverCondition(null);
    }

    private void RewardAllPlayersBasedOnDay()
    {
        if (GameModeManager.Instance.IsPvPMode)
        {
            int pot = GameModeManager.Instance.PvPRewardPot;

            // Fill remaining players if they weren't recorded
            foreach (var player in playerEntities)
            {
                if (!eliminationOrder.Contains(player.OwnerClientId))
                {
                    eliminationOrder.Add(player.OwnerClientId);
                }
            }

            var uniqueOrder = eliminationOrder
            .Distinct()
            .Reverse()
            .ToList();

        for (int i = 0; i < uniqueOrder.Count; i++)
        {
            ulong clientId = uniqueOrder[i];
            float percentage = 0f;

            if (i == 0) percentage = 0.80f;
            else if (i == 1) percentage = 0.15f;
            else if (i == 2) percentage = 0.05f;

            int reward = Mathf.RoundToInt(pot * percentage);

            // Update networked coin display
            for (int j = 0; j < LobbyPlayerList.Instance.Players.Count; j++)
            {
                if (LobbyPlayerList.Instance.Players[j].ClientId == clientId)
                {
                    var updated = LobbyPlayerList.Instance.Players[j];
                    updated.CoinsEarned = reward;
                    LobbyPlayerList.Instance.Players[j] = updated;
                    break;
                }
            }

            // Send reward only to correct client
            if (reward > 0)
            {
                RewardPvPWinnerClientRpc(reward, clientId, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                });
            }

            Debug.Log($"🏅 Player {clientId} gets {reward} coins ({percentage * 100}%)");
        }
        }
        else
        {
            // Normal co-op reward
            int day = DayManager.Instance.CurrentDayInt;
            float baseReward = 10f;
            float growthMultiplier = 1.25f;
            int reward = Mathf.RoundToInt(baseReward * Mathf.Pow(growthMultiplier, day));

            for (int i = 0; i < LobbyPlayerList.Instance.Players.Count; i++)
            {
                var updated = LobbyPlayerList.Instance.Players[i];
                updated.CoinsEarned = reward;
                LobbyPlayerList.Instance.Players[i] = updated;
            }

            RewardClientsClientRpc(reward);
        }
    }

    [ClientRpc]
    private void RewardClientsClientRpc(int reward)
    {
        CurrencyManager.Instance?.Add(reward);
        Debug.Log($"🟢 You received {reward} coins!");
    }

    [ClientRpc]
    private void SetCanInteractClientRpc(bool value)
    {
        PlayerInput.CanInteract = value;
    }

    [ClientRpc]
    private void RewardPvPWinnerClientRpc(int reward, ulong intendedRecipientClientId, ClientRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.LocalClientId != intendedRecipientClientId)
        {
            // ❌ Not for this client
            return;
        }

        CurrencyManager.Instance?.Add(reward);
        Debug.Log($"💸 You (Client {intendedRecipientClientId}) received the PvP pot of {reward} coins!");
    }

    [ClientRpc]
    private void ShowGameOverEffectClientRpc()
    {
        if (FadeScreenEffect.Instance != null)
        {
            FadeScreenEffect.Instance.ShowDeathEffect(3.7f); // Fade to deep red in 4 seconds
        }
    }
}
