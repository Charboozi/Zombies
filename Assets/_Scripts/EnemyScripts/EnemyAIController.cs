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

        targeting.OnTargetAcquired += (target) => {
            movement.SetTarget(target);
            attack.SetTarget(target);
        };

        targeting.OnTargetInRange += () => {
            attack.TargetInRange();
        };

        targeting.OnTargetLost += () => {
            movement.ClearTarget();
            attack.SetTarget(null);
        };

        targeting.OnTargetOutOfRange += () => {
            attack.StopAttack();
        };
    }

    private void Update()
    {
        if (!IsServer) return;

        movement?.TickMovement(); 
    }
}
