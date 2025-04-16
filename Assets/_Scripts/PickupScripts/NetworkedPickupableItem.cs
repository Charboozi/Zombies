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
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (IsServer)
        {
            Debug.Log($"✅ Server despawning pickup: {gameObject.name}");
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
