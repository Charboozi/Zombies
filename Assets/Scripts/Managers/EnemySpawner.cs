using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public List<Transform> spawnPoints; // List of spawn locations
    public GameObject enemyPrefab; // Assign the enemy prefab in the Inspector
    public float spawnInterval = 5f; // Time between spawns
    public int maxEnemies = 10; // Maximum number of enemies

    private List<GameObject> activeEnemies = new List<GameObject>(); // Track spawned enemies

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server handles enemy spawning
        {
            StartCoroutine(SpawnEnemies());
        }
    }

    private IEnumerator SpawnEnemies()
    {
        while (true) // Infinite loop to keep spawning
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

        // Select a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        // Spawn the enemy on the server
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemy.GetComponent<NetworkObject>().Spawn(true); // Spawn over the network

        // Add to active enemy list
        activeEnemies.Add(enemy);

        // Remove from the list when destroyed
        enemy.GetComponent<EnemyAI>().OnEnemyDeath += () => activeEnemies.Remove(enemy);
    }
}
