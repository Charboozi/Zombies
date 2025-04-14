using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class WeaponUpgradeStation : MonoBehaviour, IInteractableAction
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem upgradeEffect;

    public void DoAction()
    {
        var currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
        if (currentWeapon == null)
        {
            Debug.LogWarning("No current weapon to upgrade.");
            return;
        }

        if (!currentWeapon.CanUpgrade)
        {
            Debug.Log("Weapon has no upgrade available.");
            return;
        }

        // Get references
        WeaponBase upgradedWeapon = currentWeapon.upgradeWeapon;
        var weaponSwitcher = WeaponSwitcher.Instance;
        var weaponInventory = weaponSwitcher.GetComponent<WeaponInventory>();

        if (weaponInventory == null || weaponSwitcher == null)
        {
            Debug.LogError("Weapon system references missing.");
            return;
        }

        // Remove current weapon from inventory
        weaponInventory.RemoveWeapon(weaponSwitcher.CurrentWeaponIndex);

        // Add upgraded weapon to inventory
        weaponInventory.AddWeapon(upgradedWeapon);

        // ✅ Play upgrade particle effect
        if (upgradeEffect != null)
        {
            upgradeEffect.Play();
        }

        // Optional: play sound / notification
        Debug.Log($"🔧 Weapon upgraded to: {upgradedWeapon.name}");
    }
}
