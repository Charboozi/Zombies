using UnityEngine;

public class HandPositionController : MonoBehaviour
{
    [Header("References")]
    public WeaponSwitcher weaponSwitcher; // Reference to the WeaponSwitcher script
    public Transform oneHandedPosition; // Position for one-handed weapons
    public Transform twoHandedPosition; // Position for two-handed weapons

    private void Start()
    {
        if (weaponSwitcher == null)
        {
            Debug.LogError("❌ WeaponSwitcher reference not set in HandPositionController!");
            return;
        }

        // Subscribe to weapon switch event
        weaponSwitcher.OnWeaponSwitched += UpdateHandPosition;

        // Initialize hand position based on the currently equipped weapon
        UpdateHandPosition();
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnWeaponSwitched -= UpdateHandPosition;
        }
    }

    private void UpdateHandPosition()
    {
        if (weaponSwitcher.activeWeapons.Count == 0)
        {
            Debug.LogWarning("⚠️ No weapons active! Hand position unchanged.");
            return;
        }

        // Get the currently equipped weapon
        GameObject currentWeapon = weaponSwitcher.activeWeapons[weaponSwitcher.currentWeaponIndex];

        // Determine hand position based on weapon tag
        if (currentWeapon.CompareTag("TwoHanded"))
        {
            Debug.Log($"🟢 Switched to TWO-HANDED weapon: {currentWeapon.name}");
            MoveHandTo(twoHandedPosition);
        }
        else if (currentWeapon.CompareTag("OneHanded"))
        {
            Debug.Log($"🟢 Switched to ONE-HANDED weapon: {currentWeapon.name}");
            MoveHandTo(oneHandedPosition);
        }
        else
        {
            Debug.LogWarning($"⚠️ Weapon '{currentWeapon.name}' has NO valid tag! Defaulting to One-Handed.");
            MoveHandTo(oneHandedPosition);
        }
    }

    private void MoveHandTo(Transform targetPosition)
    {
        if (targetPosition == null)
        {
            Debug.LogError("❌ Hand position target is not set!");
            return;
        }

        transform.position = targetPosition.position;
        transform.rotation = targetPosition.rotation;
    }
}
