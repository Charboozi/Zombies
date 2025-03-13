using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI reserveAmmoText;

    void Update()
    {
        // Get the active weapon from the global WeaponManager singleton.
        WeaponBase activeWeapon = WeaponManager.Instance != null ? WeaponManager.Instance.CurrentWeapon : null;
        if (activeWeapon != null)
        {
            // Update the UI with the current ammo and reserve ammo.
            currentAmmoText.text = activeWeapon.currentAmmo.ToString();
            reserveAmmoText.text = activeWeapon.reserveAmmo.ToString();
        }
    }
}
