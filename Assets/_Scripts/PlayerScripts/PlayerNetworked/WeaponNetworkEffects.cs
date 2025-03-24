using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponNetworkEffects : NetworkBehaviour
{
    public ParticleSystem muzzleFlash;
    private bool isFiring = false;

    void OnEnable()
    {
        WeaponController.OnShoot -= TriggerMuzzleFlash;
        WeaponController.OnShoot += TriggerMuzzleFlash;

        WeaponController.OnStopShooting -= StopFiring;
        WeaponController.OnStopShooting += StopFiring;
    }
    void OnDisable()
    {
        WeaponController.OnShoot -= TriggerMuzzleFlash;
        WeaponController.OnStopShooting -= StopFiring;
    }

    public void TriggerMuzzleFlash()
    {
        if (!IsOwner) return;

        WeaponBase currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;

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
        if (CurrentWeaponHolder.Instance?.CurrentWeapon is ContinuousFireWeapon) return;
        RequestMuzzleFlashServerRpc(false);
    }

    public void StopFiring()
    {
        if (!IsOwner) return;

        if (CurrentWeaponHolder.Instance?.CurrentWeapon is ContinuousFireWeapon && isFiring)
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
}
