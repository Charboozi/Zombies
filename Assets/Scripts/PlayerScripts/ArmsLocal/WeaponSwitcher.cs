using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Active Weapons Settings")]
    // List of weapons the player has picked up.
    public List<WeaponBase> activeWeapons = new List<WeaponBase>();
    public int maxWeapons = 3;
    public int currentWeaponIndex = 0;

    // Optional: if you want to attach weapons to a specific transform (e.g., player's right hand)
    public Transform rightHand;

    public event System.Action OnWeaponSwitched;

    void Start()
    {
        // Initially, ensure all weapons are inactive.
        foreach (WeaponBase weapon in activeWeapons)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(false);
        }

        // Equip first weapon if any exist.
        if (activeWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        // Direct equip using number keys.
        if (Input.GetKeyDown(KeyCode.Alpha1) && activeWeapons.Count > 0)
            EquipWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) && activeWeapons.Count > 1)
            EquipWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3) && activeWeapons.Count > 2)
            EquipWeapon(2);
        // Cycle weapons with mouse scroll wheel.
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            CycleWeapon(1);
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            CycleWeapon(-1);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= activeWeapons.Count)
        {
            Debug.LogWarning("Weapon index out of range.");
            return;
        }

        // Set the parent of the right hand to the active weapon.
        if (rightHand != null)
        {
            rightHand.SetParent(activeWeapons[index].transform);
        }

        // Deactivate all active weapons.
        foreach (WeaponBase weapon in activeWeapons)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(false);
        }

        // Activate the chosen weapon.
        activeWeapons[index].gameObject.SetActive(true);
        currentWeaponIndex = index;
        Debug.Log("Equipped weapon: " + activeWeapons[index].name);

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.SetWeapon(activeWeapons[index]);
        }

        OnWeaponSwitched?.Invoke();
    }

    public void CycleWeapon(int direction)
    {
        if (activeWeapons.Count == 0) return;
        currentWeaponIndex = (currentWeaponIndex + direction + activeWeapons.Count) % activeWeapons.Count;
        EquipWeapon(currentWeaponIndex);
    }

    /// <summary>
    /// Picks up a weapon by name.
    /// Searches among the children (even inactive) for a WeaponBase whose name matches.
    /// </summary>
    public void PickUpWeapon(string weaponName)
    {
        // Search among this object's children for a matching weapon.
        WeaponBase weaponToAdd = GetComponentsInChildren<WeaponBase>(true)
            .FirstOrDefault(w => w.name == weaponName);

        if (weaponToAdd == null)
        {
            Debug.LogWarning($"Weapon '{weaponName}' not found among children.");
            return;
        }

        // If the weapon is already in inventory, do nothing.
        if (activeWeapons.Contains(weaponToAdd))
        {
            Debug.Log($"Weapon '{weaponName}' is already in inventory. No need to pick up again.");
            return;
        }

        // If max weapons reached, drop the **currently equipped weapon**
        if (activeWeapons.Count >= maxWeapons)
        {
            Debug.Log("Remove current weapon");
            RemoveWeapon(currentWeaponIndex); // Remove the equipped weapon
        }

        // Equip the newly picked up weapon.
        activeWeapons.Add(weaponToAdd);
        EquipWeapon(activeWeapons.IndexOf(weaponToAdd));
        Debug.Log("Picked up weapon: " + weaponToAdd.name);
    }

        /// <summary>
    /// Removes a weapon from active weapons and deactivates it.
    /// </summary>
    private void RemoveWeapon(int index)
    {
        if (index >= 0 && index < activeWeapons.Count)
        {
            WeaponBase weapon = activeWeapons[index];
            activeWeapons.RemoveAt(index);
            weapon.gameObject.SetActive(false);
            Debug.Log("Removed weapon: " + weapon.name);
        }
    }

    //Not used right now maybe later
    public void DropWeapon(int index)
    {
        if (index >= 0 && index < activeWeapons.Count)
        {
            WeaponBase weapon = activeWeapons[index];
            activeWeapons.RemoveAt(index);
            weapon.gameObject.SetActive(false);
            Debug.Log("Dropped weapon: " + weapon.name);

            if (activeWeapons.Count > 0)
            {
                currentWeaponIndex = index % activeWeapons.Count;
                EquipWeapon(currentWeaponIndex);
            }
        }
    }

    public int GetActiveWeaponID()
    {
        return activeWeapons.Count > 0 ? currentWeaponIndex : -1;
    }
}
