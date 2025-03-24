using UnityEngine;
using Unity.Netcode;

public class PowerupSpawner : NetworkBehaviour
{
    [Range(0f, 1f)] public float spawnChance = 0.3f;
    private Vector3 localSpawnOffset = new Vector3(0, -1f, 0);

    private GameObject spawnedPowerup;

    public void TrySpawn()
    {
        if (!IsServer) return;
        if (Random.value > spawnChance) return;

        GameObject[] powerups = Resources.LoadAll<GameObject>("Powerups");
        if (powerups.Length == 0) return;

        GameObject chosen = powerups[Random.Range(0, powerups.Length)];

        spawnedPowerup = Instantiate(chosen, transform.position + localSpawnOffset, Quaternion.Euler(-90f, 0f, 0f));
        spawnedPowerup.transform.SetParent(transform);

        NetworkObject netObj = spawnedPowerup.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(true);
        }
        else
        {
            Debug.LogError("Powerup prefab missing NetworkObject.");
        }
    }
}
