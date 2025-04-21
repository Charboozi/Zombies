using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkedPickupableItem : NetworkBehaviour
{
    [SerializeField] private float autoDespawnTime = 10f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(AutoDespawnRoutine());
        }
    }

    private IEnumerator AutoDespawnRoutine()
    {
        yield return new WaitForSeconds(autoDespawnTime);
        Despawn();
    }

    public void Despawn()
    {
        if (IsOwner && pickupSound != null)
        {
            // Try to get the local player object (owned by this client)
            var localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (localPlayer != null && localPlayer.TryGetComponent<AudioSource>(out var audioSource))
            {
                // Play as 2D (make sure AudioSource on the player has spatialBlend = 0)
                audioSource.PlayOneShot(pickupSound);
            }
        }

        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
        else
        {
            RequestDespawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc()
    {
        Debug.Log($"🔹 ServerRpc received to despawn pickup: {gameObject.name}");
        GetComponent<NetworkObject>().Despawn();
    }
}
