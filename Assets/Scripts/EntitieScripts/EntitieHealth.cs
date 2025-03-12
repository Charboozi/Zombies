using UnityEngine;
using Unity.Netcode;

public class EntityHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server sets up health
        {
            currentHealth.Value = maxHealth;
        }
    }

    //Client that got hit to server:"I got shot"
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        if (currentHealth.Value <= 0) return; // Already dead

        TakeDamageClientRpc();

        currentHealth.Value -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth.Value}");

        if (currentHealth.Value <= 0)
        {
            DieClientRpc();
        }
    }

    //Server to all clients: "This guy got damaged"
    [ClientRpc]
    public void TakeDamageClientRpc()
    {
        if(IsOwner && gameObject.tag != "Enemy")
        {
            DamageScreenEffect.Instance.ShowDamageEffect();
        }
    }

    //Server to all clients"This guy died"
    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"{gameObject.name} has died!");
        if(IsOwner && gameObject.tag != "Enemy")
        {
            DamageScreenEffect.Instance.ShowDeathEffect();
            RequestDespawnServerRpc();
        }

        if(gameObject.tag == "Enemy")
        {
            EnemyAI enemyAI = GetComponent<EnemyAI>();
            enemyAI.PlayDeathAnimation();
        }
    }

    //Temporary death for player
    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true); // Despawn the player for all clients
        }
    }

}
