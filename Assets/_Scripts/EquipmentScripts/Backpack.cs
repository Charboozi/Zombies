using UnityEngine;

public class Backpack : MonoBehaviour
{
    [SerializeField] private int weaponBonus = 2;
    [SerializeField] private WeaponInventory weaponInventory;

    private void OnEnable()
    {
        if (weaponInventory != null)
        {
            weaponInventory.SetMaxWeapons(weaponInventory.WeaponCount + weaponBonus);
        }
    }
}
