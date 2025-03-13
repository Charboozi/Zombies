using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; } // Singleton for easy access
    public WeaponBase CurrentWeapon { get; private set; } // Tracks active weapon

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SetWeapon(WeaponBase weapon)
    {
        CurrentWeapon = weapon;
    }
}
