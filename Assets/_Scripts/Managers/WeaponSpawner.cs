using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponSpawner : NetworkBehaviour
{
    [Header("Weapon Spawning Settings")]
    public List<GameObject> weaponPrefabs;
    public float respawnTime = 10f;

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server should spawn weapons
        {
            SpawnWeapon();
        }
    }

    private void SpawnWeapon()
    {
        if (weaponPrefabs.Count == 0)
        {
            Debug.LogWarning("No weapons assigned to WeaponSpawner!");
            return;
        }

        int randomIndex = Random.Range(0, weaponPrefabs.Count);
        GameObject weaponInstance = Instantiate(weaponPrefabs[randomIndex], transform.position, transform.rotation);

        // Spawn the weapon across the network
        NetworkObject networkObject = weaponInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();

            RenameWeaponClientRpc(networkObject.NetworkObjectId);
        }
    }

    [ClientRpc]
    private void RenameWeaponClientRpc(ulong weaponId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(weaponId, out NetworkObject weapon))
        {
            weapon.gameObject.name = weapon.gameObject.name.Replace("(Clone)", "").Trim();
            Debug.Log($"✅ Renamed weapon on client: {weapon.gameObject.name}");
        }
    }
}
