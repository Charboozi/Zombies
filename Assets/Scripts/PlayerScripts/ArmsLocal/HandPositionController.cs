using UnityEngine;

public class HandPositionController : MonoBehaviour
{
    [Header("Hand Position Targets")]
    public Transform oneHandedPosition;  // Position for one-handed weapons
    public Transform twoHandedPosition;  // Position for two-handed weapons

    private void Start()
    {
        if (WeaponManager.Instance == null)
        {
            Debug.LogError("❌ WeaponManager instance not found! Ensure a WeaponManager is in the scene.");
            return;
        }
        
        if (FindFirstObjectByType<WeaponSwitcher>() != null)
        {
            FindFirstObjectByType<WeaponSwitcher>().OnWeaponSwitched += UpdateHandPosition;
        }

        // Initialize hand position based on the currently equipped weapon.
        UpdateHandPosition();

    }


    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks.
        if (FindFirstObjectByType<WeaponSwitcher>() != null)
        {
            FindFirstObjectByType<WeaponSwitcher>().OnWeaponSwitched -= UpdateHandPosition;
        }
    }

    public void UpdateHandPosition()
    {
        // Retrieve the current weapon from the global WeaponManager.
        WeaponBase activeWeapon = WeaponManager.Instance.CurrentWeapon;
        if (activeWeapon == null)
        {
            Debug.LogWarning("⚠️ No active weapon found in WeaponManager. Hand position unchanged.");
            return;
        }

        // Use the GameObject's tag to determine the appropriate hand position.
        if (activeWeapon.gameObject.CompareTag("TwoHanded"))
        {
            Debug.Log($"🟢 Switched to TWO-HANDED weapon: {activeWeapon.name}");
            MoveHandTo(twoHandedPosition);
        }
        else if (activeWeapon.gameObject.CompareTag("OneHanded"))
        {
            Debug.Log($"🟢 Switched to ONE-HANDED weapon: {activeWeapon.name}");
            MoveHandTo(oneHandedPosition);
        }
        else
        {
            Debug.LogWarning($"⚠️ Weapon '{activeWeapon.name}' has NO valid tag! Defaulting to One-Handed.");
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
