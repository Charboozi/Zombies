using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(TargetScanner), typeof(EnemyAttack))]
public class EnemyAIController : NetworkBehaviour
{
    private IEnemyMovement movement;
    private TargetScanner targeting;
    private EnemyAttack attack;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        movement = GetComponent<IEnemyMovement>();
        targeting = GetComponent<TargetScanner>();
        attack = GetComponent<EnemyAttack>();

        targeting.OnTargetAcquired += movement.SetTarget;
        targeting.OnTargetAcquired += attack.SetTarget;
        targeting.OnTargetInRange += attack.StartAttack;
        targeting.OnTargetLost += movement.ClearTarget;
    }

    private void Update()
    {
        if (!IsServer) return;

        movement?.TickMovement(); 
    }
}
