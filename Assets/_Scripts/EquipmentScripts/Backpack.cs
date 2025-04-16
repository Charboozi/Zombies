using UnityEngine;

public class Backpack : BaseEquipment
{
    [SerializeField] private int weaponBonus = 2;
    [SerializeField] private WeaponInventory weaponInventory;

    private int originalMaxWeapons;

    private void OnEnable()
    {
        if (weaponInventory != null)
        {
            originalMaxWeapons = weaponInventory.WeaponCount + weaponBonus;
            weaponInventory.SetMaxWeapons(originalMaxWeapons);
        }
    }

    private void OnDisable()
    {
        if (weaponInventory != null)
        {
            // Revert back to original count before bonus
            int newMax = Mathf.Max(weaponInventory.WeaponCount, originalMaxWeapons - weaponBonus);
            weaponInventory.SetMaxWeapons(newMax);
        }
    }
}
