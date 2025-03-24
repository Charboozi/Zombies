using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : NetworkBehaviour, IEnemyMovement
{
    [Header("Roaming")]
    public float roamRadius = 10f;
    public float roamDelay = 3f;

    private NavMeshAgent agent;
    private Transform currentTarget;
    private float roamTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (!IsServer) return;

        PickNewRoamDestination();
    }

    public void TickMovement()
    {
        if (!IsServer || agent == null) return;

        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            roamTimer -= Time.deltaTime;

            if (roamTimer <= 0f || agent.remainingDistance < 1f)
                PickNewRoamDestination();
        }
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    private void PickNewRoamDestination()
    {
        Vector3 randomPoint = Random.insideUnitSphere * roamRadius + transform.position;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            roamTimer = roamDelay;
        }
    }
}
