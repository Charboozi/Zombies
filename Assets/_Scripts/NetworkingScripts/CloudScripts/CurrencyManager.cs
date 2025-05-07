using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;
    public int Coins { get; private set; }

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

    public async Task InitAsync()
    {
        // ✅ Only proceed if the player is signed in (handled by UnityAuthManager)
        if (!UnityAuthManager.Instance || !UnityAuthManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("⚠️ CurrencyManager InitAsync() called before authentication!");
            return;
        }

        await LoadCoinsAsync();
    }


    public async Task LoadCoinsAsync()
    {
        Debug.Log("🔄 Attempting to load coins...");

        var keys = new HashSet<string> { "coins" };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        Debug.Log($"📦 CloudSave returned {data.Count} keys");

        if (data.TryGetValue("coins", out var item))
        {
            string coinStr = item.Value.GetAsString();
            Debug.Log($"👉 Raw coin value: {coinStr}");

            if (int.TryParse(coinStr, out int loadedCoins))
            {
                Coins = loadedCoins;
                Debug.Log($"✅ Coins loaded: {Coins}");
            }
            else
            {
                Debug.LogError("❌ Could not parse coin value!");
                Coins = 0;
            }
        }
        else
        {
            Debug.Log("🆕 No coins key found. Starting at 0.");
            Coins = 0;
        }
    }

    public async Task SaveCoinsAsync()
    {
        Debug.Log($"💾 Saving coins: {Coins}");
        var saveData = new Dictionary<string, object>
        {
            { "coins", Coins }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
        Debug.Log("✅ Save successful");
    }

    public void Add(int amount)
    {
        Coins += amount;
        Debug.Log($"➕ Added {amount} coins. New total: {Coins}");
        _ = SaveCoinsAsync();
    }

    public bool Spend(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            Debug.Log($"➖ Spent {amount} coins. Remaining: {Coins}");
            _ = SaveCoinsAsync();
            return true;
        }

        Debug.LogWarning("❌ Not enough coins to spend.");
        return false;
    }
}
