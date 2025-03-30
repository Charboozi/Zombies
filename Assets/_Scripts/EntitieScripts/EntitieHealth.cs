using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class EntityHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int armor = 0;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Regeneration Settings")]
    public bool enableRegeneration = true; // Toggle regeneration on/off
    public int regenAmount = 2; // Health regained per tick
    public float regenInterval = 2f; // Time between each regeneration tick

    private Coroutine regenCoroutine; // Store coroutine to avoid duplicates
    
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
        int effectiveDamage = damage - armor;
        if (effectiveDamage < 1)
            effectiveDamage = 1;

        TakeDamageClientRpc();

        currentHealth.Value -= effectiveDamage;
        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth.Value}");

        if (currentHealth.Value <= 0)
        {
            DieClientRpc();
        }
        RestartHealthRegen();
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

    private void StartHealthRegen()
    {
        if (IsServer && enableRegeneration && gameObject.tag != "Enemy")
        {
            if (regenCoroutine == null) // Ensure we don't start multiple coroutines
            {
                regenCoroutine = StartCoroutine(HealthRegenCoroutine());
            }
        }
    }

    private void RestartHealthRegen()
    {
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }
        Invoke(nameof(StartHealthRegen), regenInterval * 2); // Wait before restarting regen
    }

    // Coroutine to regenerate health over time
    private IEnumerator HealthRegenCoroutine()
    {
        while (currentHealth.Value < maxHealth)
        {
            yield return new WaitForSeconds(regenInterval); // Wait before adding health

            if (currentHealth.Value < maxHealth)
            {
                currentHealth.Value += regenAmount;
                if (currentHealth.Value > maxHealth) currentHealth.Value = maxHealth; // Prevent over-healing

                Debug.Log($"{gameObject.name} regenerated {regenAmount} health. Current: {currentHealth.Value}");
            }
        }

        regenCoroutine = null; // Stop coroutine when full health is reached
    }

    //Server to all clients"This guy died"
    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"{gameObject.name} has died!");
        if(IsOwner && gameObject.tag == "Player")
        {
            DamageScreenEffect.Instance.ShowDeathEffect();
            RequestDespawnServerRpc();
        }

        if(TryGetComponent<IKillable>(out var killable))
        {
            killable.Die();
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

    // Methods to modify armor.
    public void AddArmor(int bonus)
    {
        armor += bonus;
        Debug.Log($"{gameObject.name}: Armor increased by {bonus}. Total armor: {armor}");
    }

    public void RemoveArmor(int bonus)
    {
        armor -= bonus;
        if (armor < 0) armor = 0;
        Debug.Log($"{gameObject.name}: Armor decreased by {bonus}. Total armor: {armor}");
    }

}
