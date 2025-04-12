using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

[RequireComponent(typeof(Interactable))]
public class TeleporterInteractable : NetworkBehaviour, IInteractableAction
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform[] teleportPoints;

    private List<GameObject> playersInZone = new List<GameObject>();

    public void DoAction()
    {
        if (!IsServer) return;

        if (playersInZone.Count == 0)
        {
            Debug.LogWarning("[Teleport] No players in zone, teleportation skipped.");
            return;
        }

        Debug.Log($"[Teleport] Starting teleportation for {playersInZone.Count} players.");

        foreach (var player in playersInZone)
        {
            if (player != null)
            {
                Transform randomPoint = teleportPoints[Random.Range(0, teleportPoints.Length)];
                NetworkObject playerNetObj = player.GetComponent<NetworkObject>();

                if (playerNetObj != null)
                {
                    TeleportPlayerClientRpc(playerNetObj.OwnerClientId, randomPoint.position);
                    Debug.Log($"[Teleport] Teleporting {player.name} to {randomPoint.position}");
                }
                else
                {
                    Debug.LogWarning($"[Teleport] Player {player.name} has no NetworkObject!");
                }
            }
        }

        Debug.Log("[Teleport] Teleportation complete. Clearing playersInZone list.");
        playersInZone.Clear();
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong targetClientId, Vector3 position)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        var controller = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()?.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false; // Disable to avoid physics issues
            controller.transform.position = position;
            controller.enabled = true;
        }
        else
        {
            Debug.LogWarning("[Teleport] No CharacterController found on player!");
        }

        if (FlashEffect.Instance != null)
        {
            FlashEffect.Instance.TriggerFlash();
        }
        else
        {
            Debug.LogWarning("[Teleport] FlashEffect.Instance is null!");
        }
    }

    // Trigger additions from external trigger script
    public void AddPlayerToZone(GameObject player)
    {
        if (!playersInZone.Contains(player))
        {
            playersInZone.Add(player);
            Debug.Log($"[Teleport] Player added: {player.name} | Total: {playersInZone.Count}");
        }
    }

    public void RemovePlayerFromZone(GameObject player)
    {
        if (playersInZone.Contains(player))
        {
            playersInZone.Remove(player);
            Debug.Log($"[Teleport] Player removed: {player.name} | Total: {playersInZone.Count}");
        }
    }
}
