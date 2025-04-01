using UnityEngine;
using System.Linq;

[RequireComponent(typeof(WeaponInventory))]
public class WeaponSwitcher : MonoBehaviour
{
    public int CurrentWeaponIndex { get; private set; } = 0;

    [SerializeField] private Transform rightHand;
    [SerializeField] private WeaponController weaponController;
    
    public event System.Action OnWeaponSwitched;

    private WeaponInventory inventory;

    void Awake()
    {
        inventory = GetComponent<WeaponInventory>();
        inventory.OnWeaponAdded += HandleWeaponAdded;
        inventory.OnWeaponLimitReached += HandleWeaponLimitReached;
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnWeaponAdded -= HandleWeaponAdded;
            inventory.OnWeaponLimitReached -= HandleWeaponLimitReached;
        }
    }

    private void OnEnable()
    {
        PlayerInput.OnSwitchWeapon += EquipWeapon;
        PlayerInput.OnCycleWeapon += CycleWeapon;
    }

    private void OnDisable()
    {
        PlayerInput.OnSwitchWeapon -= EquipWeapon;
        PlayerInput.OnCycleWeapon -= CycleWeapon;
    }

    void Start()
    {
        foreach (WeaponBase weapon in inventory.Weapons)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(false);
        }

        if (inventory.Weapons.Count > 0)
            EquipWeapon(0);
    }

    public void EquipWeapon(int index)
    {
        weaponController.OnWeaponSwitched();
        if (index < 0 || index >= inventory.Weapons.Count)
        {
            Debug.LogWarning("Weapon index out of range.");
            return;
        }

        if (rightHand != null)
        {
            rightHand.SetParent(inventory.Weapons[index].transform);
        }

        foreach (WeaponBase weapon in inventory.Weapons)
        {
            if (weapon != null)
                weapon.gameObject.SetActive(false);
        }

        inventory.Weapons[index].gameObject.SetActive(true);
        CurrentWeaponIndex = index;

        if (CurrentWeaponHolder.Instance != null)
        {
            CurrentWeaponHolder.Instance.SetWeapon(inventory.Weapons[index]);
        }

        Debug.Log("Equipped weapon: " + inventory.Weapons[index].name);
        OnWeaponSwitched?.Invoke();
    }

    public void CycleWeapon(int direction)
    {
        if (inventory.Weapons.Count == 0) return;
        int newIndex = (CurrentWeaponIndex + direction + inventory.Weapons.Count) % inventory.Weapons.Count;
        EquipWeapon(newIndex);
    }

    private void HandleWeaponLimitReached(int max)
    {
        inventory.RemoveWeapon(CurrentWeaponIndex);
    }

    private void HandleWeaponAdded(WeaponBase newWeapon)
    {
        int index = inventory.Weapons.ToList().IndexOf(newWeapon);
        EquipWeapon(index);
    }
}
