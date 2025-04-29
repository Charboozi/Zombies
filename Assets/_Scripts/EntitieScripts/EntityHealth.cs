using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;

public class EntityHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int armor = 0;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isDowned = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Regeneration Settings")]
    public bool enableRegeneration = true; // Toggle regeneration on/off
    public int regenAmount = 2; // Health regained per tick
    public float regenInterval = 2f; // Time between each regeneration tick

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioSource audioSource;

    private Coroutine regenCoroutine; // Store coroutine to avoid duplicates

    public event Action OnTakeDamage;

    private CameraShakeController cameraShake;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cameraShake = FindFirstObjectByType<CameraShakeController>();
        }

        if (IsServer) // Only the server sets up health
        {
            currentHealth.Value = maxHealth;
            isDowned.Value = false;
        }

        if (IsServer && CompareTag("Player"))
        {
            GameOverManager.Instance?.RegisterPlayer(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && CompareTag("Player"))
        {
            GameOverManager.Instance?.UnregisterPlayer(this);
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
        OnTakeDamage?.Invoke();
        //Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth.Value}");

        // If health reaches 0, determine whether to down or die.
        if (currentHealth.Value <= 0)
        {
            if (gameObject.CompareTag("Player"))
            {
                // Set player to downed state (only if not already downed).
                if (!isDowned.Value)
                {
                    isDowned.Value = true;
                    DownedClientRpc();
                }
            }
            else
            {
                DieClientRpc();
            }
        }

        RestartHealthRegen();

        if(gameObject.tag == "Player")
        {
            ApplySlowEffectClientRpc(0.3f, 2f);
        }
    }

    //Server to all clients: "This guy got damaged"
    [ClientRpc]
    public void TakeDamageClientRpc()
    {
        if(IsOwner && gameObject.tag != "Enemy")
        {
            FadeScreenEffect.Instance.ShowEffect(Color.red, 0.5f, 4f);

            if (damageSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(damageSound);
            }

            if (cameraShake != null)
            {
                cameraShake.Shake(0.1f, 0.15f);
            }
        }
    }

    private void StartHealthRegen()
    {
        if (IsServer && enableRegeneration && gameObject.tag != "Enemy" && !isDowned.Value)
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
                if (currentHealth.Value > maxHealth) 
                currentHealth.Value = maxHealth; // Prevent over-healing
            }
        }
        regenCoroutine = null; // Stop coroutine when full health is reached
    }

    //Server to all clients"This guy died"
    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"{gameObject.name} has died!");

        if(TryGetComponent<IKillable>(out var killable))
        {
            killable.Die();
        }
    }

    
    // ClientRpc to show the downed state for players.
    [ClientRpc]
    private void DownedClientRpc()
    {
        Debug.Log($"{gameObject.name} is downed!");
        if (IsOwner && gameObject.CompareTag("Player"))
        {
            // Replace this with your downed animation or visual effect.
            FadeScreenEffect.Instance.ShowDownedEffect();

            PlayerInput.CanInteract = false; // 🔥 Disable interact button
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

    [ClientRpc]
    void ApplySlowEffectClientRpc(float slowFactor, float duration)
    {
        if (IsOwner)
        {
            var movement = GetComponent<NetworkedCharacterMovement>();
            if (movement != null)
            {
                movement.ApplyTemporarySlow(slowFactor, duration);
            }
        }
    }

    // Call this method from your revival system to heal and re-enable the player.
    public void FullHeal()
    {
        if (gameObject.CompareTag("Player"))
        {
            currentHealth.Value = maxHealth;
            isDowned.Value = false; // Revive the player
            PlayReviveEffectClientRpc();
            Debug.Log($"{gameObject.name}: Fully healed to {maxHealth} and revived.");

            EnableInteractionClientRpc();
        }
    }

    public void Revive()
    {
        if (gameObject.CompareTag("Player"))
        {
            currentHealth.Value = Mathf.Min(10, maxHealth); // ✅ 10 HP revive for players
            isDowned.Value = false; // ✅ Stand up again
            PlayReviveEffectClientRpc();
            Debug.Log($"{gameObject.name}: Revived with 10 HP.");

            EnableInteractionClientRpc();
        }
    }

    [ClientRpc]
    private void EnableInteractionClientRpc()
    {
        if (IsOwner)
        {
            PlayerInput.CanInteract = true;
        }
    }


    [ClientRpc]
    private void PlayReviveEffectClientRpc()
    {
        if (IsOwner && gameObject.CompareTag("Player"))
        {
            FadeScreenEffect.Instance.ShowReviveEffect();
        }
    }

    /// <summary>
    /// Server‐only damage application. Call this from server code.
    /// </summary>
    public void ApplyDamage(int amount)
    {
        if (!IsServer) return;

        int effective = Mathf.Max(amount - armor, 1);
        currentHealth.Value -= effective;
        OnTakeDamage?.Invoke();
        TakeDamageClientRpc();

        if (currentHealth.Value <= 0)
        {
            if (CompareTag("Player") && !isDowned.Value)
            {
                isDowned.Value = true;
                DownedClientRpc();
            }
            else if (!CompareTag("Player"))
            {
                DieClientRpc();
            }
        }

        // restart regen, slow effect, etc.
        RestartHealthRegen();
        if (CompareTag("Player"))
            ApplySlowEffectClientRpc(0.3f, 2f);
    }

}
