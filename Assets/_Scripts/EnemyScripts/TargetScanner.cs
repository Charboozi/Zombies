using UnityEngine;
using System;
using Unity.Netcode;

/// <summary>
/// Scans for the closest valid target within a detection range and emits events.
/// Can be used for enemies, turrets, or other AI units.
/// </summary>
public class TargetScanner : NetworkBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask targetLayer;

    private Transform currentTarget;
    private float checkInterval = 1f;
    private float checkTimer = 0f;

    public event Action<Transform> OnTargetAcquired;
    public event Action OnTargetLost;
    public event Action OnTargetInRange;

    private EnemyAttack enemyAttack;

    void Awake()
    {
        enemyAttack = GetComponent<EnemyAttack>();
    }

    void Update()
    {
        if (!IsServer || enemyAttack == null) return;

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;
            ScanForTargets();
        }

        if (currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.position);

            if (dist > detectionRange)
            {
                OnTargetLost?.Invoke();
                currentTarget = null;
            }
            else if (dist <= enemyAttack.attackRange)
            {
                OnTargetInRange?.Invoke();
            }
        }
    }

    private void ScanForTargets()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRange, targetLayer);

        Transform closestTarget = null;
        float closestDist = Mathf.Infinity;

        foreach (var candidate in targets)
        {
            float dist = Vector3.Distance(transform.position, candidate.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestTarget = candidate.transform;
            }
        }

        if (closestTarget != null && closestTarget != currentTarget)
        {
            currentTarget = closestTarget;
            OnTargetAcquired?.Invoke(closestTarget);
        }
    }
}
