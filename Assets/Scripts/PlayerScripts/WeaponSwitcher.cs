using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Active Weapons Settings")]
    // Only weapons that have been picked up will be in this list.
    public List<GameObject> activeWeapons = new List<GameObject>();
    public int maxWeapons = 3;
    public int currentWeaponIndex = 0;

    public Transform rightHand;

    public event System.Action OnWeaponSwitched;

    void Start()
    {
        // Ensure all weapons in the holder start inactive
        foreach (Transform weapon in gameObject.transform)
        {
            weapon.gameObject.SetActive(false);
        }

        // Equip first weapon if available
        if (activeWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
    }

    void Update()
    {
        // Direct equip using number keys (if enough active weapons)
        if (Input.GetKeyDown(KeyCode.Alpha1) && activeWeapons.Count > 0)
        {
            EquipWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && activeWeapons.Count > 1)
        {
            EquipWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && activeWeapons.Count > 2)
        {
            EquipWeapon(2);
        }
        // Cycle weapons with mouse scroll wheel
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            CycleWeapon(1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            CycleWeapon(-1);
        }
    }
    
    void EquipWeapon(int index)
    {
        rightHand.SetParent(activeWeapons[index].transform);
        if (index < activeWeapons.Count)
        {
            // Deactivate all active weapons
            foreach (GameObject weapon in activeWeapons)
            {
                weapon.SetActive(false);
            }
            // Activate the selected weapon
            activeWeapons[index].SetActive(true);
            currentWeaponIndex = index;
            Debug.Log("Equipped weapon ID: " + GetWeaponID(activeWeapons[index]));

            // Trigger weapon switched event
            OnWeaponSwitched?.Invoke();
        }
    }

    void CycleWeapon(int direction)
    {
        if (activeWeapons.Count == 0) return;
        currentWeaponIndex = (currentWeaponIndex + direction + activeWeapons.Count) % activeWeapons.Count;
        EquipWeapon(currentWeaponIndex);
    }

    // Returns the weapon ID based on its sibling index in the weapon holder.
    int GetWeaponID(GameObject weapon)
    {
        return weapon.transform.GetSiblingIndex();
    }

    // Call this method when picking up a weapon.
    // The weapon parameter should be a reference to one of the children of weaponHolder.
    public void PickUpWeapon(string weaponName)
    {
        // Find a child GameObject with the same name as the picked-up weapon
        Transform matchingChild = transform.Find(weaponName);

        if (matchingChild != null)
        {
            GameObject weaponObject = matchingChild.gameObject;

            // Add weapon to the active weapons list
            activeWeapons.Add(weaponObject);

            // Equip the new weapon immediately
            EquipWeapon(activeWeapons.IndexOf(weaponObject));

            Debug.Log($"Picked up and equipped weapon: {weaponName} (Index: {currentWeaponIndex})");
        }
        else
        {
            Debug.LogWarning($"No matching child found for weapon: {weaponName}");
        }
    }



    // Optionally, you can add a method to drop a weapon.
    public void DropWeapon(int index)
    {
        if (index >= 0 && index < activeWeapons.Count)
        {
            GameObject weapon = activeWeapons[index];
            activeWeapons.RemoveAt(index);
            weapon.SetActive(false);
            Debug.Log("Dropped weapon ID: " + GetWeaponID(weapon));

            // Re-equip a weapon if there are any left
            if (activeWeapons.Count > 0)
            {
                currentWeaponIndex = index % activeWeapons.Count;
                EquipWeapon(currentWeaponIndex);
            }
        }
    }

    // Returns the active weapon's ID, or -1 if no weapon is active.
    public int GetActiveWeaponID()
    {
        if (activeWeapons.Count > 0)
        {
            return GetWeaponID(activeWeapons[currentWeaponIndex]);
        }
        return -1;
    }
}
