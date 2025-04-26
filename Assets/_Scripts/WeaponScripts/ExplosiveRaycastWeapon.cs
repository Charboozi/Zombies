using UnityEngine;

public class ExplosiveRaycastWeapon : WeaponBase
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Shoot()
    {
        if (!CanShoot()) return;

        currentAmmo--;
        UpdateEmissionIntensity();

        var origin    = playerCamera.transform.position;
        var direction = playerCamera.transform.forward;
        if (Physics.Raycast(origin, direction, out var hit, range))
        {
            // spawn the VFX everywhere
            NetworkImpactSpawner.Instance?
                .SpawnImpactEffectServerRpc(hit.point, hit.normal, "ExplosionImpact");

            // now tell the server “explode here”
            ExplosionManager.Instance.ExplodeServerRpc(hit.point);
        }

        muzzleFlash?.Play();
    }

}
