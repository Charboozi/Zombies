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
        while (isActive)
        {
            FindNewTarget();

            if (currentTarget != null && firingCoroutine == null)
            {
                firingCoroutine = StartCoroutine(FireAtTarget());
            }
            else if (currentTarget == null && firingCoroutine != null)
            {
                StopCoroutine(firingCoroutine);
                firingCoroutine = null;
            }

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
                // Optional: skip dead enemies
                if (entity.currentHealth.Value <= 0)
                    continue;

                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = hit.transform;
                }
            }
        }

        currentTarget = bestTarget;
    }

    private IEnumerator FireAtTarget()
    {
        // Initial delay to avoid instant raycast spike
        yield return new WaitForSeconds(0.3f);

        while (currentTarget != null && isActive)
        {
            Vector3 direction = (currentTarget.position - muzzlePoint.position).normalized;
            Ray ray = new Ray(muzzlePoint.position, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, detectionRange))
            {
                if (hit.collider.TryGetComponent<EntityHealth>(out var victim))
                {
                    victim.TakeDamageServerRpc(damagePerShot);
                }

                // Play effects
                PlayMuzzleEffectClientRpc();
                SpawnImpactEffectClientRpc(hit.point, hit.normal);
            }

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
