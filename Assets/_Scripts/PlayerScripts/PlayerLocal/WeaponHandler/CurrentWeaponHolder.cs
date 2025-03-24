using UnityEngine;

public class CurrentWeaponHolder : MonoBehaviour
{
    public static CurrentWeaponHolder Instance { get; private set; } 
    public WeaponBase CurrentWeapon { get; private set; } 

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
