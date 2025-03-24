using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    private Slider healthBar; // Automatically assigned
    private EntityHealth playerHealth;

    void Awake()
    {
        // Get the Slider component from the same GameObject
        healthBar = GetComponent<Slider>();

        if (healthBar == null)
        {
            Debug.LogError("Slider component missing from " + gameObject.name);
            return;
        }

        // Find the local player (owner)
        EntityHealth[] allEntities = FindObjectsByType<EntityHealth>(FindObjectsSortMode.None);
        foreach (var entity in allEntities)
        {
            if (entity.IsOwner) // Only track local player's health
            {
                playerHealth = entity;
                playerHealth.currentHealth.OnValueChanged += UpdateHealthUI;
                UpdateHealthUI(0, playerHealth.currentHealth.Value); // Initial UI update
                break;
            }
        }
    }

    private void UpdateHealthUI(int oldValue, int newValue)
    {
        if (healthBar != null && playerHealth != null)
        {
            healthBar.value = (float)newValue / playerHealth.maxHealth;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.currentHealth.OnValueChanged -= UpdateHealthUI;
        }
    }
}
