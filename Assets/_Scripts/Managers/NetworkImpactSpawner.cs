using UnityEngine;
using Unity.Netcode;

public class NetworkImpactSpawner : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public void SpawnImpactEffectServerRpc(Vector3 position, Vector3 normal, string prefabName)
    {
        SpawnImpactEffectClientRpc(position, normal, prefabName);
    }

    [ClientRpc]
    private void SpawnImpactEffectClientRpc(Vector3 position, Vector3 normal, string prefabName)
    {
        // Load the impact effect prefab from Resources
        GameObject impactEffectPrefab = Resources.Load<GameObject>($"ImpactEffects/{prefabName}");
        
        if (impactEffectPrefab != null)
        {
            GameObject impactEffect = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(impactEffect, 2f); // Auto-destroy after 2 seconds
        }
        else
        {
            Debug.LogError($"Impact effect prefab '{prefabName}' not found in Resources/ImpactEffects/");
        }
    }
}
