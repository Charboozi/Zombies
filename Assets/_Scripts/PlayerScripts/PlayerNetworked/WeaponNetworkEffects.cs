using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WeaponNetworkEffects : NetworkBehaviour
{
    [Header("Effects")]
    public ParticleSystem muzzleFlash;

    [Header("Camera")]
    [SerializeField] private CameraShakeController cameraShake;

    private bool isFiring = false;
    private bool isLoopingWeapon = false;

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

    // Ensure remote instances start with no particles alive
    public override void OnNetworkSpawn()
    {
        if (!IsOwner && muzzleFlash != null)
        {
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void TriggerMuzzleFlash()
    {
        if (!IsOwner) return;

        WeaponBase currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        if (currentWeapon == null) return;

        isLoopingWeapon = currentWeapon is ContinuousFireWeapon;

        if (isLoopingWeapon)
        {
            if (!isFiring)
            {
                isFiring = true;
                RequestMuzzleFlashServerRpc(true);
            }
        }
        else
        {
            // one-shot: play then schedule stop after fireRate
            RequestMuzzleFlashServerRpc(true);
            Invoke(nameof(StopMuzzleFlash), currentWeapon.fireRate);
        }

        // Always shake camera on owner
        if (cameraShake != null)
        {
            cameraShake.Shake(0.06f, 0.14f);
        }
    }

    private void StopMuzzleFlash()
    {
        if (isLoopingWeapon)
        {
            if (isFiring)
            {
                isFiring = false;
                RequestMuzzleFlashServerRpc(false);
            }
        }
        else
        {
            RequestMuzzleFlashServerRpc(false);
        }
    }

    public void StopFiring()
    {
        if (!IsOwner) return;

        if (isLoopingWeapon && isFiring)
        {
            isFiring = false;
            RequestMuzzleFlashServerRpc(false);
        }
    }

    [ServerRpc]
    private void RequestMuzzleFlashServerRpc(bool playEffect, ServerRpcParams rpcParams = default)
    {
        // Send to all other clients
        var targetIds = new List<ulong>();
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != rpcParams.Receive.SenderClientId)
                targetIds.Add(clientId);
        }

        var rpcParamsOut = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = targetIds.ToArray() }
        };

        PlayMuzzleFlashClientRpc(playEffect, rpcParamsOut);
    }

    [ClientRpc]
    private void PlayMuzzleFlashClientRpc(bool playEffect, ClientRpcParams clientRpcParams = default)
    {
        if (muzzleFlash == null) return;

        if (playEffect)
        {
            // Reset and play (works for both looping and one-shot)
            muzzleFlash.Clear(true);
            muzzleFlash.Play(true);
        }
        else
        {
            // Stop and clear all particles
            muzzleFlash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
