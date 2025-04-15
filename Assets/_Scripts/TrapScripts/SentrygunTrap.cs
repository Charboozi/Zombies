using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(Interactable))]
public class SentrygunTrap : TrapBase
{
    [Header("Turret Parts")]
    [SerializeField] private Transform turretHead;
    [SerializeField] private Transform muzzlePoint;

    [Header("Effects")]
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private GameObject impactEffectPrefab;

    [Header("Turret Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float fireInterval = 1f;
    [SerializeField] private float scanInterval = 3f;
    [SerializeField] private int damagePerShot = 15;

    [Header("Detection")]
    [SerializeField] private LayerMask enemyLayer;

    private Transform currentTarget;
    private Coroutine firingCoroutine;
    private Coroutine scanningCoroutine;

    private void Update()
    {
        if (!IsServer || !isActive || currentTarget == null) return;

        Vector3 direction = (currentTarget.position - turretHead.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        turretHead.rotation = Quaternion.Lerp(turretHead.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    protected override void OnTrapActivated()
    {
        currentTarget = null;

        // Random startup delay to spread activation spikes
        float randomDelay = Random.Range(2f, 3f);
        Invoke(nameof(StartScanning), randomDelay);
    }

    private void StartScanning()
    {
        scanningCoroutine = StartCoroutine(ScanForTargets());
    }

    protected override void OnTrapDeactivated()
    {
        if (scanningCoroutine != null)
        {
            StopCoroutine(scanningCoroutine);
            scanningCoroutine = null;
        }

        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }

        currentTarget = null;

        if (muzzleEffect != null && muzzleEffect.isPlaying)
        {
            muzzleEffect.Stop();
        }
    }

    private IEnumerator ScanForTargets()
    {
        if (firingCoroutine == null)
        {
            firingCoroutine = StartCoroutine(FireContinuously());
        }

        while (isActive)
        {
            FindNewTarget();
            yield return new WaitForSeconds(scanInterval);
        }
    }


    private void FindNewTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);

        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EntityHealth>(out var entity))
            {
                if (entity.currentHealth.Value <= 0)
                    continue;

                Vector3 dirToTarget = (hit.transform.position - muzzlePoint.position).normalized;
                float distToTarget = Vector3.Distance(muzzlePoint.position, hit.transform.position);

                // 🔎 Line of sight check
                if (Physics.Raycast(muzzlePoint.position, dirToTarget, out RaycastHit rayHit, distToTarget))
                {
                    if (rayHit.transform != hit.transform)
                    {
                        // Something is in the way (wall, etc.)
                        continue;
                    }
                }

                if (distToTarget < closestDistance)
                {
                    closestDistance = distToTarget;
                    bestTarget = hit.transform;
                }
            }
        }

        currentTarget = bestTarget;
    }


    private IEnumerator FireContinuously()
    {
        yield return new WaitForSeconds(0.3f); // optional initial delay

        while (isActive)
        {
            Vector3 direction = muzzlePoint.forward;
            Ray ray = new Ray(muzzlePoint.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))
            {
                if (hit.collider.TryGetComponent<EntityHealth>(out var victim))
                {
                    victim.TakeDamageServerRpc(damagePerShot);
                }

                SpawnImpactEffectClientRpc(hit.point, hit.normal);
            }

            PlayMuzzleEffectClientRpc();

            yield return new WaitForSeconds(fireInterval);
        }
    }


    [ClientRpc]
    private void PlayMuzzleEffectClientRpc()
    {
        if (muzzleEffect != null)
        {
            muzzleEffect.Play();
        }
    }

    [ClientRpc]
    private void SpawnImpactEffectClientRpc(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab != null)
        {
            var impact = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(impact, 2f);
        }
    }

    private void Reset()
    {
        // Auto-assign enemy layer on reset
        enemyLayer = LayerMask.GetMask("Enemy");
    }
}
