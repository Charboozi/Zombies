using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WeaponNetworkEffects : NetworkBehaviour
{
    public ParticleSystem muzzleFlash;
    private ProceduralWeaponAnimation proceduralWeaponAnimation;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindWeaponShooterScript();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find the script in case of scene changes
        FindWeaponShooterScript();
    }

    // This method is triggered by the shooting event.
    // Only the owner calls this, and then it requests the server to play the effect on all other clients.
    public void TriggerMuzzleFlash()
    {
        if (IsOwner)
        {
            RequestMuzzleFlashServerRpc();
        }
    }

    [ServerRpc]
    private void RequestMuzzleFlashServerRpc(ServerRpcParams rpcParams = default)
    {
        // Prepare a list of target client IDs that excludes the sender (owner)
        List<ulong> targetClientIds = new List<ulong>();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != rpcParams.Receive.SenderClientId)
            {
                targetClientIds.Add(clientId);
            }
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = targetClientIds.ToArray() }
        };

        // Invoke the ClientRpc on all non-owner clients
        PlayMuzzleFlashClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void PlayMuzzleFlashClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // All remote clients will execute this and play the muzzle flash
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    void FindWeaponShooterScript()
    {
        if (proceduralWeaponAnimation == null)
        {
            proceduralWeaponAnimation = FindFirstObjectByType<ProceduralWeaponAnimation>();
            if (proceduralWeaponAnimation != null)
            {
                // Unsubscribe first to avoid duplicate subscriptions
                proceduralWeaponAnimation.OnShoot -= TriggerMuzzleFlash;
                proceduralWeaponAnimation.OnShoot += TriggerMuzzleFlash;
            }
        }
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (proceduralWeaponAnimation != null)
        {
            proceduralWeaponAnimation.OnShoot -= TriggerMuzzleFlash;
        }
    }
}
