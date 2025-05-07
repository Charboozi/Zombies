using UnityEngine;

public class LoadPurchasedItemsIntoGame : MonoBehaviour
{
    private void Start()
    {
        if (PlayerInventoryManager.Instance == null || ConsumableManager.Instance == null)
        {
            Debug.LogWarning("❌ PlayerInventoryManager or ConsumableManager missing.");
            return;
        }

        // Add all unlocked items here
        int keycards = PlayerInventoryManager.Instance.Keycards;
        ConsumableManager.Instance.Add("Keycard", keycards);

        Debug.Log($"🟩 Injected purchased items into ConsumableManager (e.g., {keycards} keycards).");
    }
}
