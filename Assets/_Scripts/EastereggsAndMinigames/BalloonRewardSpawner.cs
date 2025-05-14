using UnityEngine;
using Unity.Netcode;

public class BalloonRewardSpawner : NetworkBehaviour
{
    [Tooltip("Possible items to randomly spawn as a reward.")]
    [SerializeField] private GameObject[] rewardPrefabs;

    [Tooltip("Offset from the spawner's position.")]
    [SerializeField] private Vector3 spawnOffset = Vector3.up * 0.5f;

    public void SpawnRandomReward()
    {
        if (!IsServer)
        {
            Debug.LogWarning("Tried to spawn reward from client! Aborted.");
            return;
        }

        if (rewardPrefabs == null || rewardPrefabs.Length == 0)
        {
            Debug.LogWarning("❌ No reward prefabs assigned.");
            return;
        }

        int index = Random.Range(0, rewardPrefabs.Length);
        GameObject prefab = rewardPrefabs[index];
        Vector3 spawnPosition = transform.position + spawnOffset;

        GameObject rewardInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        if (rewardInstance.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Spawn();
            Debug.Log($"🎁 Spawned networked reward: {prefab.name}");
        }
        else
        {
            Debug.LogError("❌ Reward prefab is missing NetworkObject component!");
        }
    }
}
