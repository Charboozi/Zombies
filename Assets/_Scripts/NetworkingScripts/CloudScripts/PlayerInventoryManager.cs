using UnityEngine;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ------------------ Keycards ------------------

    public int Keycards { get; private set; }
    private const string keycardsKey = "keycards";

    public async Task LoadKeycardsAsync()
    {
        var keys = new HashSet<string> { keycardsKey };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        if (data.TryGetValue(keycardsKey, out var item) && int.TryParse(item.Value.GetAsString(), out int value))
        {
            Keycards = value;
        }
        else
        {
            Keycards = 0;
        }

        Debug.Log($"🟢 Loaded keycards: {Keycards}");
    }

    public void AddKeycards(int amount)
    {
        Keycards += amount;
        Debug.Log($"➕ Gained {amount} keycards. New total: {Keycards}");
        _ = SaveKeycardsAsync();
    }

    public async Task SaveKeycardsAsync()
    {
        var saveData = new Dictionary<string, object>
        {
            { keycardsKey, Keycards }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
        Debug.Log($"💾 Saved keycards: {Keycards}");
    }

    // ------------------ Weapon Unlocks ------------------

    public HashSet<string> UnlockedWeapons { get; private set; } = new();
    public HashSet<string> ActiveWeapons { get; private set; } = new();

    private const string weaponsKey = "unlockedWeapons";
    private const string activeWeaponsKey = "activeWeapons";

    public int GetUnlockedWeaponTier(string baseWeaponName)
    {
        int maxTier = 0;

        if (UnlockedWeapons.Contains(baseWeaponName))
            maxTier = 1;

        for (int tier = 2; tier <= 3; tier++)
        {
            string tierName = $"{baseWeaponName} Tier {tier}";
            if (UnlockedWeapons.Contains(tierName))
                maxTier = tier;
        }

        return maxTier;
    }

    public async Task LoadUnlockedWeaponsAsync()
    {
        var keys = new HashSet<string> { weaponsKey, activeWeaponsKey };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        // Load unlocked weapons
        if (data.TryGetValue(weaponsKey, out var item))
        {
            try
            {
                string json = item.Value.GetAsString();
                var wrapper = JsonUtility.FromJson<WeaponListWrapper>(json);
                UnlockedWeapons = wrapper.weapons.ToHashSet();
            }
            catch
            {
                Debug.LogWarning("⚠️ Failed to parse unlocked weapons. Resetting.");
                UnlockedWeapons = new HashSet<string>();
            }
        }

        // Default fallback
        if (UnlockedWeapons.Count == 0)
        {
            UnlockedWeapons.Add("Pistol");
        }

        // Load active weapons
        if (data.TryGetValue(activeWeaponsKey, out var activeItem))
        {
            try
            {
                string json = activeItem.Value.GetAsString();
                var wrapper = JsonUtility.FromJson<WeaponListWrapper>(json);
                ActiveWeapons = wrapper.weapons.ToHashSet();
            }
            catch
            {
                Debug.LogWarning("⚠️ Failed to parse active weapons. Resetting.");
                ActiveWeapons = new HashSet<string>();
            }
        }
        else
        {
            ActiveWeapons = new HashSet<string>();
        }

        Debug.Log($"🟢 Loaded unlocked weapons: {string.Join(", ", UnlockedWeapons)}");
    }

    public void UnlockWeapon(string weaponName)
    {
        if (UnlockedWeapons.Add(weaponName))
        {
            Debug.Log($"✅ Unlocked weapon: {weaponName}");
            _ = SaveUnlockedWeaponsAsync();
        }
        else
        {
            Debug.Log($"🟡 Weapon already unlocked: {weaponName}");
        }
    }
    
    public bool TryToggleActiveWeapon(string weaponName)
    {
        if (!UnlockedWeapons.Contains(weaponName)) return false;

        if (ActiveWeapons.Contains(weaponName))
        {
            ActiveWeapons.Remove(weaponName);
            SaveAllInventoryAsync();
            return true;
        }
        else
        {
            if (ActiveWeapons.Count >= 2)
            {
                Debug.Log("⚠️ Cannot activate more than 2 weapons.");
                return false;
            }

            ActiveWeapons.Add(weaponName);
            SaveAllInventoryAsync();
            return true;
        }
    }


    public async Task SaveUnlockedWeaponsAsync()
    {
        var saveData = new Dictionary<string, object>();

        var unlockedWrapper = new WeaponListWrapper { weapons = UnlockedWeapons.ToList() };
        string unlockedJson = JsonUtility.ToJson(unlockedWrapper);
        saveData[weaponsKey] = unlockedJson;

        var activeWrapper = new WeaponListWrapper { weapons = ActiveWeapons.ToList() };
        string activeJson = JsonUtility.ToJson(activeWrapper);
        saveData[activeWeaponsKey] = activeJson;

        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
        Debug.Log("💾 Saved unlocked & active weapons.");
    }

    public void SaveAllInventoryAsync()
    {
        _ = SaveUnlockedWeaponsAsync();
        _ = SaveKeycardsAsync();
    }

    [System.Serializable]
    private class WeaponListWrapper
    {
        public List<string> weapons;
    }
}
