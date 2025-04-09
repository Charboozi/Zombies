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

    private IEnemyAnimationHandler enemyAnimation;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimation = GetComponent<IEnemyAnimationHandler>() ?? NullEnemyAnimationHandler.Instance;
    }


    private void Start()
    {
        if (!IsServer) return;

        PickNewRoamDestination();
        roamTimer = roamDelay;

        EnemyManager.Instance.RegisterEnemy(this);
    }

    public override void OnDestroy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterEnemy(this);
        }
    }

    public void TickMovement()
    {
        if (!IsServer || agent == null || !agent.isActiveAndEnabled) return;

        roamTimer -= Time.deltaTime;

        if (currentTarget == null && (roamTimer <= 0f || agent.remainingDistance < 1f))
        {
            PickNewRoamDestination();
            roamTimer = roamDelay;
        }

        UpdateMoveAnimation();
    }

    public void UpdateDestination()
    {
        if (!IsServer || agent == null || !agent.isActiveAndEnabled) return;

        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
        else if (agent.remainingDistance < 1f)
        {
            PickNewRoamDestination();
            roamTimer = roamDelay;
        }

        UpdateMoveAnimation();
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
        }
    }
    
    private void UpdateMoveAnimation()
    {
        float speed = agent.velocity.magnitude;
        enemyAnimation.SetMoveSpeed(speed);
    }
}
