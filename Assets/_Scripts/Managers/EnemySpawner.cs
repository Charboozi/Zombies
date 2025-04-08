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

    private Coroutine spawnCoroutine;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Start()
    {
        if (IsServer && DayManager.Instance != null)
        {
            DayManager.Instance.OnNewDayStarted += OnNewDayStarted;
        }
    }
    
    /// <summary>
    /// Increase difficulty each day
    /// </summary>
    private void OnNewDayStarted(int day)
    {
        spawnInterval = Mathf.Max(0.5f, spawnInterval * spawnIntervalDecreaseRate);
        maxEnemies += maxEnemiesIncreaseAmount;

        Debug.Log($"[Spawner] Day {day}: spawnInterval = {spawnInterval}, maxEnemies = {maxEnemies}");
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            spawnCoroutine = StartCoroutine(SpawnEnemies());
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
        if (spawnPoints.Count == 0 || enemyPrefab == null) return;

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
}
