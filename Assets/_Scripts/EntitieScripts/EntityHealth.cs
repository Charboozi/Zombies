using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections;

public class EntityHealth : NetworkBehaviour
{
    [Header("Special Enemy")]
    [SerializeField] private bool isSpecialEnemy = false;

    public static event Action<EntityHealth> OnSpecialEnemyKilled;
    public event Action<EntityHealth> OnDowned;

    public string lastHitLimbID = "None";

    public void OnLimbHit(string limbID, int damage)
    {
        lastHitLimbID = limbID;
        TakeDamageServerRpc(damage);
    }

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
                    OnDowned?.Invoke(this); // ✅ Notify GameOverManager
                    DownedClientRpc();
                    ShowDownedFeedMessageClientRpc(GetSteamNameFromNameTag());
                }
            }
            else
            {
                DieClientRpc();

                if (isSpecialEnemy)
                {
                    Debug.Log($"[SpecialKill] Special enemy '{gameObject.name}' killed.");
                    OnSpecialEnemyKilled?.Invoke(this);
                }
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

    [ClientRpc]
    private void DieClientRpc()
    {
        Debug.Log($"{gameObject.name} has died!");

        if (TryGetComponent<IKillable>(out var killable))
            killable.Die();

        // Detach head/core if that's what killed it
        if (lastHitLimbID.ToLower().Contains("head") || lastHitLimbID.ToLower().Contains("core"))
        {
            var limb = FindLimbByID(lastHitLimbID);
            if (limb != null)
                DetachLimb(limb);
        }
    }

    private Transform FindLimbByID(string id)
    {
        foreach (var limb in GetComponentsInChildren<LimbHealth>())
        {
            if (limb.limbID.Equals(id, StringComparison.OrdinalIgnoreCase))
                return limb.transform;
        }
        return null;
    }

    [Header("Limb Effects")]
    [SerializeField] private ParticleSystem headDetachEffect; // Assign this to an existing FX object

    private void DetachLimb(Transform limb)
    {
        // Shrink the limb visually
        limb.localScale = Vector3.one * 0.001f;

        // Add collider if needed
        if (!limb.GetComponent<Collider>())
            limb.gameObject.AddComponent<BoxCollider>();

        // If head, play effect
        if (lastHitLimbID.ToLower().Contains("head") && headDetachEffect != null)
        {
            headDetachEffect.Play();
        }
    }


    [ClientRpc]
    private void DownedClientRpc()
    {
        Debug.Log($"{gameObject.name} is downed!");

        if (IsOwner && gameObject.CompareTag("Player"))
        {
            // 🔒 Block interactions
            PlayerInput.CanInteract = false;

            // 🟥 Show downed effect
            FadeScreenEffect.Instance.ShowDownedEffect();

            // 🎥 PvP mode => switch to TrailerFreeCam
            if (GameModeManager.Instance != null && GameModeManager.Instance.IsPvPMode)
            {
                var freeCam = FindFirstObjectByType<TrailerFreeCam>();
                if (freeCam != null)
                {
                    Debug.Log("🎥 Switching to TrailerFreeCam after PvP downed.");
                    freeCam.SendMessage("ActivateFreeCam", SendMessageOptions.DontRequireReceiver);
                }
            }
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
            RestartHealthRegen();
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
            RestartHealthRegen();
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
                OnDowned?.Invoke(this); // ✅ Notify GameOverManager
                DownedClientRpc();
                ShowDownedFeedMessageClientRpc(GetSteamNameFromNameTag());
            }
            else if (!CompareTag("Player"))
            {
                DieClientRpc();

                if (isSpecialEnemy)
                {
                    Debug.Log($"[SpecialKill] Special enemy '{gameObject.name}' killed.");
                    OnSpecialEnemyKilled?.Invoke(this);
                }
            }
        }

        // restart regen, slow effect, etc.
        RestartHealthRegen();
        if (CompareTag("Player"))
            ApplySlowEffectClientRpc(0.3f, 2f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ApplyHealingServerRpc(int amount)
    {
        if (currentHealth.Value <= 0 || isDowned.Value) return;

        currentHealth.Value += amount;
        if (currentHealth.Value > maxHealth)
            currentHealth.Value = maxHealth;

        PlayHealingEffectClientRpc();
    }

    [ClientRpc]
    private void PlayHealingEffectClientRpc()
    {
        if (IsOwner)
        {
            FadeScreenEffect.Instance.ShowEffect(Color.green, 0.05f, 1f);
        }
    }

    private string GetSteamNameFromNameTag()
    {
        // Try to find the PlayerNameTag component up the hierarchy
        var current = transform;
        while (current != null)
        {
            if (current.TryGetComponent<PlayerNameTag>(out var tag))
            {
                string name = tag.GetPlayerName();
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }
            current = current.parent;
        }

        // Fallback: use SteamManager fallback or default
        if (SteamManager.Instance != null)
        {
            return SteamManager.Instance.GetPlayerName(); // Handles fallback internally
        }

        return $"DemoPlayer_{OwnerClientId}";
    }

    [ClientRpc]
    private void ShowDownedFeedMessageClientRpc(string playerName)
    {
        GameFeedManager.Instance?.PostFeedMessageClientRpc($"{playerName} is downed!");
    }
}
