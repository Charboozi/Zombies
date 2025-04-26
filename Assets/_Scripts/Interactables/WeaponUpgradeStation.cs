using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class WeaponUpgradeStation : MonoBehaviour, IClientOnlyAction
{
    [Header("Effects")]
    [SerializeField] private ParticleSystem upgradeEffect;

public void DoClientAction()
{
    var currentWeapon = CurrentWeaponHolder.Instance?.CurrentWeapon;
    if (currentWeapon == null)
    {
        Debug.LogWarning("No current weapon to upgrade.");
        return;
    }

    if (!currentWeapon.CanUpgrade)
    {
        Debug.Log("❌ This weapon cannot be upgraded (upgradeWeapon is null).");
        return;
    }

    var upgradedWeapon = currentWeapon.upgradeWeapon;
    if (upgradedWeapon == null)
    {
        Debug.LogError("❌ Upgrade failed: upgraded weapon reference is missing.");
        return;
    }

    var weaponSwitcher = WeaponSwitcher.Instance;
    var weaponInventory = weaponSwitcher.GetComponent<WeaponInventory>();

    if (weaponInventory == null || weaponSwitcher == null)
    {
        Debug.LogError("Weapon system references missing.");
        return;
    }

    // Deactivate and remove old weapon
    currentWeapon.gameObject.SetActive(false);
    weaponInventory.RemoveWeapon(weaponSwitcher.CurrentWeaponIndex);

    // Add upgraded weapon
    weaponInventory.AddWeapon(upgradedWeapon);

    // Play upgrade particle effect
    if (upgradeEffect != null)
    {
        upgradeEffect.Play();
    }

    Debug.Log($"🔧 Weapon upgraded locally to: {upgradedWeapon.name}");
}

}
