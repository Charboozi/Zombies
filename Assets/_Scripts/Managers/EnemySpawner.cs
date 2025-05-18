using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints;
    public GameObject enemyPrefab;
    [Tooltip("Time between spawns (in seconds)")]
    public float spawnInterval = 5f;
    public int maxEnemies = 10;

    [Header("Difficulty Scaling")]
    [Tooltip("Multiplier to reduce spawn interval per day (e.g., 0.95 = 5% faster each day)")]
    public float spawnIntervalDecreaseRate = 0.95f;
    [Tooltip("How many enemies to add each day")]
    public int maxEnemiesIncreaseAmount = 2;

    [Header("Activation Settings")]
    [Tooltip("Day this spawner becomes active")]
    public int activateOnDay = 0;

    private Coroutine spawnCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawnerActive = false;

    private void Start()
    {
        if (IsServer && DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted += OnNewDayStarted;

            // If the current day is already past the start day, activate immediately
            if (DayManager.Instance.CurrentDayInt >= activateOnDay)
            {
                ActivateSpawner();
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && DayManager.Instance.CurrentDayInt >= activateOnDay)
        {
            ActivateSpawner();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        if (DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted -= OnNewDayStarted;
        }
    }

    private void OnNewDayStarted(int day)
    {
        if (day >= activateOnDay && !isSpawnerActive)
        {
            ActivateSpawner();
        }

        if (isSpawnerActive)
        {
            // Gradually slow down difficulty scaling

            // 1. Reduce spawn interval (but less and less each day)
            spawnIntervalDecreaseRate = Mathf.Lerp(spawnIntervalDecreaseRate, 1f, 0.03f); // 3% closer to 1 each day
            spawnInterval = Mathf.Max(0.5f, spawnInterval * spawnIntervalDecreaseRate);

            // 2. Reduce enemy growth (slower maxEnemies increase)
            if (maxEnemiesIncreaseAmount > 1)
            {
                maxEnemiesIncreaseAmount = Mathf.Max(1, Mathf.RoundToInt(maxEnemiesIncreaseAmount * 0.95f)); // 5% reduction
            }
            maxEnemies += maxEnemiesIncreaseAmount;

            Debug.Log($"[Spawner] Day {day}: spawnInterval = {spawnInterval}, maxEnemies = {maxEnemies}, spawnIntervalDecreaseRate = {spawnIntervalDecreaseRate}, maxEnemiesIncreaseAmount = {maxEnemiesIncreaseAmount}");
        }
    }

    private void ActivateSpawner()
    {
        if (isSpawnerActive || spawnPoints.Count == 0 || enemyPrefab == null) return;

        AdjustDifficultyForPlayerCount(); // <-- Add this line

        isSpawnerActive = true;
        spawnCoroutine = StartCoroutine(SpawnEnemies());
        Debug.Log($"[Spawner] Activated on Day {DayManager.Instance.CurrentDayInt}");
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<NetworkObject>().Spawn(true);
        activeEnemies.Add(enemy);

        var deathHandler = enemy.GetComponent<EnemyDeathHandler>();
        if (deathHandler != null)
        {
            deathHandler.OnEnemyDeath += () => activeEnemies.Remove(enemy);
        }
        else
        {
            Debug.LogWarning("Spawned enemy missing EnemyDeathHandler component!");
        }
    }

    private void AdjustDifficultyForPlayerCount()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (playerCount < 4 && playerCount >= 2)
        {
            spawnIntervalDecreaseRate = Mathf.Lerp(1f, spawnIntervalDecreaseRate, 0.5f); // Bring it closer to 1
            maxEnemiesIncreaseAmount = Mathf.Max(1, maxEnemiesIncreaseAmount / 2);

            Debug.Log($"[Spawner] Adjusted difficulty for {playerCount} players: spawnIntervalDecreaseRate = {spawnIntervalDecreaseRate}, maxEnemiesIncreaseAmount = {maxEnemiesIncreaseAmount}");
        }
        if (playerCount < 2)
        {
            spawnIntervalDecreaseRate = Mathf.Lerp(1f, spawnIntervalDecreaseRate, 0.75f); // Bring it closer to 1
            maxEnemiesIncreaseAmount = Mathf.Max(1, maxEnemiesIncreaseAmount / 3);

            Debug.Log($"[Spawner] Adjusted difficulty for {playerCount} players: spawnIntervalDecreaseRate = {spawnIntervalDecreaseRate}, maxEnemiesIncreaseAmount = {maxEnemiesIncreaseAmount}");
        }
    }
}
