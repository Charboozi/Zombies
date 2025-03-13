using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class WeaponNetworkEffects : NetworkBehaviour
{
    public ParticleSystem muzzleFlash;
    private ProceduralWeaponAnimation proceduralWeaponAnimation;
    private bool isFiring = false;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindWeaponShooterScript();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindWeaponShooterScript();
    }

    public void TriggerMuzzleFlash()
    {
        if (!IsOwner) return;

        WeaponBase currentWeapon = WeaponManager.Instance?.CurrentWeapon;

        if (currentWeapon is ContinuousFireWeapon)
        {
            if (!isFiring)
            {
                isFiring = true;
                RequestMuzzleFlashServerRpc(true);
            }
        }
        else
        {
            // ✅ Fix for automatic weapons (ensures effect is triggered every shot)
            RequestMuzzleFlashServerRpc(true);
            Invoke(nameof(StopMuzzleFlash), currentWeapon.fireRate); // Ensures flash stops correctly
        }
    }

    private void StopMuzzleFlash()
    {
        if (WeaponManager.Instance?.CurrentWeapon is ContinuousFireWeapon) return;
        RequestMuzzleFlashServerRpc(false);
    }

    public void StopFiring()
    {
        if (!IsOwner) return;

        if (WeaponManager.Instance?.CurrentWeapon is ContinuousFireWeapon && isFiring)
        {
            isFiring = false;
            RequestMuzzleFlashServerRpc(false);
        }
    }

    [ServerRpc]
    private void RequestMuzzleFlashServerRpc(bool playEffect, ServerRpcParams rpcParams = default)
    {
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

        PlayMuzzleFlashClientRpc(playEffect, clientRpcParams);
    }

    [ClientRpc]
    private void PlayMuzzleFlashClientRpc(bool playEffect, ClientRpcParams clientRpcParams = default)
    {
        if (muzzleFlash != null)
        {
            if (playEffect)
            {
                if (!muzzleFlash.isPlaying)
                    muzzleFlash.Play();
            }
            else
            {
                if (muzzleFlash.isPlaying)
                    muzzleFlash.Stop();
            }
        }
    }

    void FindWeaponShooterScript()
    {
        if (proceduralWeaponAnimation == null)
        {
            proceduralWeaponAnimation = FindFirstObjectByType<ProceduralWeaponAnimation>();
            if (proceduralWeaponAnimation != null)
            {
                proceduralWeaponAnimation.OnShoot -= TriggerMuzzleFlash;
                proceduralWeaponAnimation.OnShoot += TriggerMuzzleFlash;

                proceduralWeaponAnimation.OnStopShooting -= StopFiring;
                proceduralWeaponAnimation.OnStopShooting += StopFiring;
            }
        }
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (proceduralWeaponAnimation != null)
        {
            proceduralWeaponAnimation.OnShoot -= TriggerMuzzleFlash;
            proceduralWeaponAnimation.OnStopShooting -= StopFiring;
        }
    }
}
