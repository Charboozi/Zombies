using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Interactable))]
public class TeleporterInteractable : NetworkBehaviour, IInteractableAction
{
    [Header("Normal Teleport Points")]
    [SerializeField] private Transform[] teleportPoints;

    [Header("Reward Teleport Points")]
    [SerializeField] private Transform[] rewardTeleportPoints;

    [Header("Reward Room Spawners")]
    [SerializeField] private BalloonRewardSpawner[] rewardSpawners;

    [Header("Teleporter Visuals")]
    [SerializeField] private GameObject normalVisual;
    [SerializeField] private GameObject rewardVisual;

    private List<GameObject> playersInZone = new List<GameObject>();
    private Dictionary<ulong, Vector3> playerRewardDestinations = new Dictionary<ulong, Vector3>();

    private bool isOverrideActive = false;
    private bool rewardUsed = false;
    private float rewardDuration = 0f;

    // Called by ChallengeInteractable on success
    public void OverrideDestinationTemporarily(float duration)
    {
        // Assign destinations
        playerRewardDestinations.Clear();
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        for (int i = 0; i < clients.Count && i < rewardTeleportPoints.Length; i++)
            playerRewardDestinations[clients[i].ClientId] = rewardTeleportPoints[i].position;

        // Flip visuals to “reward mode”
        normalVisual?.SetActive(false);
        rewardVisual?.SetActive(true);

        isOverrideActive = true;
        rewardUsed = false;
        rewardDuration = duration;

        Debug.Log($"[Teleporter] Override enabled—waiting for use. Duration after use: {duration}s.");
    }

    public void DoAction()
    {
        if (!IsServer || playersInZone.Count == 0) return;

        foreach (var player in playersInZone)
        {
            if (player == null) continue;
            var netObj = player.GetComponent<NetworkObject>();
            if (netObj == null) continue;

            // <-- Fix starts here
            Vector3 rewardPoint = Vector3.zero; 
            bool useReward = isOverrideActive 
                            && !rewardUsed 
                            && playerRewardDestinations.TryGetValue(netObj.OwnerClientId, out rewardPoint);
            // <-- Fix ends here

            Vector3 destination;
            if (useReward)
            {
                // spawn N rewards once
                int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
                int spawnCount = Mathf.Min(playerCount, rewardSpawners.Length);
                for (int i = 0; i < spawnCount; i++)
                    rewardSpawners[i]?.SpawnRandomReward();

                destination = rewardPoint;
                
                // mark override used, flip visuals, start return timer
                rewardUsed = true;
                normalVisual?.SetActive(true);
                rewardVisual?.SetActive(false);
                StartCoroutine(EndOverrideAfterDelay());
                Debug.Log("[Teleporter] Reward used—starting return timer.");
            }
            else
            {
                destination = teleportPoints[Random.Range(0, teleportPoints.Length)].position;
            }

            TeleportPlayerClientRpc(netObj.OwnerClientId, destination);
        }

        playersInZone.Clear();
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong targetClientId, Vector3 position)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;
        var ctrl = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject()
                   ?.GetComponent<CharacterController>();
        if (ctrl != null)
        {
            ctrl.enabled = false;
            ctrl.transform.position = position;
            ctrl.enabled = true;
        }
        FlashEffect.Instance?.TriggerFlash();
    }

    private IEnumerator EndOverrideAfterDelay()
    {
        yield return new WaitForSeconds(rewardDuration);

        // Teleport all back to normal points
        foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
        {
            var pos = teleportPoints[Random.Range(0, teleportPoints.Length)].position;
            TeleportPlayerClientRpc(c.ClientId, pos);
        }

        // Reset override state
        isOverrideActive = false;
        rewardUsed = false;
        playerRewardDestinations.Clear();

        Debug.Log("[Teleporter] Return timer done—back to normal mode.");
    }

    public void AddPlayerToZone(GameObject player)
    {
        if (!playersInZone.Contains(player))
            playersInZone.Add(player);
    }

    public void RemovePlayerFromZone(GameObject player)
    {
        playersInZone.Remove(player);
    }
}
