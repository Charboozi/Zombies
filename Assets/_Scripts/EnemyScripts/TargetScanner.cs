using UnityEngine;
using System;
using Unity.Netcode;

public class TargetScanner : NetworkBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float inRangeThreshold = 2f;
    [SerializeField] private LayerMask targetLayer;

    private Transform currentTarget;
    private float checkInterval = 1f;
    private float checkTimer = 0f;

    public event Action<Transform> OnTargetAcquired;
    public event Action OnTargetLost;
    public event Action OnTargetInRange;
    public event Action OnTargetOutOfRange;

    private void Update()
    {
        if (!IsServer) return;

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
            else if (dist <= inRangeThreshold)
            {
                OnTargetInRange?.Invoke();
            }
            else
            {
                OnTargetOutOfRange?.Invoke();
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
            // Check if the candidate has EntityHealth and is not downed
            if (candidate.TryGetComponent<EntityHealth>(out var entityHealth))
            {
                if (entityHealth.isDowned.Value)
                    continue; // Skip downed targets
            }
            else
            {
                continue; // Skip if no EntityHealth (optional safety)
            }
            
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