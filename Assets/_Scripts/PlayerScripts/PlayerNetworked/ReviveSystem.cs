using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ReviveSystem : NetworkBehaviour
{
    [Tooltip("Time required to complete revival")]
    public float reviveTime = 3f;

    [Tooltip("Distance within which revival is possible.")]
    public float reviveRange = 2f;

    private bool isReviving = false;

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryFindAndReviveNearbyPlayer();
        }
    }

    private void TryFindAndReviveNearbyPlayer()
    {
        if (isReviving) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, reviveRange);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EntityHealth>(out var entityHealth))
            {
                if (entityHealth.isDowned.Value && hit.gameObject != gameObject) // Don't revive self
                {
                    StartCoroutine(ReviveCoroutine(hit.gameObject, entityHealth));
                    break; // Only revive one player at a time
                }
            }
        }
    }

    private IEnumerator ReviveCoroutine(GameObject downedPlayer, EntityHealth entityHealth)
    {
        isReviving = true;

        float timer = 0f;
        while (timer < reviveTime)
        {
            // Optionally update UI progress here.
            timer += Time.deltaTime;
            yield return null;
        }

        RevivePlayerServerRpc(downedPlayer.GetComponent<NetworkObject>().NetworkObjectId);

        isReviving = false;
    }

    [ServerRpc]
    private void RevivePlayerServerRpc(ulong downedNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(downedNetworkObjectId, out NetworkObject no))
        {
            var entityHealth = no.GetComponent<EntityHealth>();
            if (entityHealth != null && entityHealth.isDowned.Value)
            {
                entityHealth.FullHeal();
                Debug.Log($"Player {downedNetworkObjectId} revived.");
            }
        }
    }
}
