using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PlayerModelWeaponManager : NetworkBehaviour
{
    private WeaponSwitcher weaponSwitcher;
    public Transform weaponParent; // Parent object containing weapon slots (0,1,2,...)

    private NetworkVariable<int> activeWeaponID = new NetworkVariable<int>(
        -1, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindWeaponSwitcher();

        // Subscribe to the network variable change to update visuals only when necessary
        if (IsClient)
        {
            activeWeaponID.OnValueChanged += OnWeaponIDChanged;
        }
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (weaponSwitcher != null)
        {
            weaponSwitcher.OnWeaponSwitched -= RequestWeaponSwitch;
        }
        if (IsClient)
        {
            activeWeaponID.OnValueChanged -= OnWeaponIDChanged;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find the WeaponSwitcher in the new scene if needed
        FindWeaponSwitcher();
    }

    void FindWeaponSwitcher()
    {
        if (weaponSwitcher == null)
        {
            weaponSwitcher = FindFirstObjectByType<WeaponSwitcher>();
            if (weaponSwitcher != null)
            {
                // Unsubscribe first to prevent duplicate subscriptions
                weaponSwitcher.OnWeaponSwitched -= RequestWeaponSwitch;
                weaponSwitcher.OnWeaponSwitched += RequestWeaponSwitch;
            }
        }
    }

    // Callback for when the network variable changes
    void OnWeaponIDChanged(int oldValue, int newValue)
    {
        UpdateWeaponSlots(newValue);
    }

    void UpdateWeaponSlots(int weaponID)
    {
        if (weaponParent == null) return;
        for (int i = 0; i < weaponParent.childCount; i++)
        {
            Transform weaponSlot = weaponParent.GetChild(i);
            bool shouldBeActive = i == weaponID;
            // Only update if the active state is different
            if (weaponSlot.gameObject.activeSelf != shouldBeActive)
            {
                weaponSlot.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    // Called by WeaponSwitcher when a weapon switch happens
    void RequestWeaponSwitch()
    {
        if (IsOwner && weaponSwitcher != null)
        {
            int newWeaponID = weaponSwitcher.GetActiveWeaponID();
            // Only send an RPC if the new weapon differs from the current one
            if (newWeaponID != activeWeaponID.Value)
            {
                RequestWeaponSwitchServerRpc(newWeaponID);
            }
        }
    }

    [ServerRpc]
    void RequestWeaponSwitchServerRpc(int newWeaponID)
    {
        activeWeaponID.Value = newWeaponID;
    }
}
