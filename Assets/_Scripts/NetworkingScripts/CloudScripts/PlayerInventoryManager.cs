using UnityEngine;
using Unity.Services.CloudSave;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerInventoryManager : MonoBehaviour
{
    public static PlayerInventoryManager Instance;

    public int Keycards { get; private set; }


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

    public async Task LoadInventoryAsync()
    {
        Debug.Log("🔄 Loading keycards...");

        var keys = new HashSet<string> { "keycards" };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(keys);

        if (data.TryGetValue("keycards", out var item))
        {
            if (int.TryParse(item.Value.GetAsString(), out int value))
            {
                Keycards = value;
                Debug.Log($"🟢 Loaded keycards: {Keycards}");
            }
        }
        else
        {
            Keycards = 0;
            Debug.Log("🆕 No keycards key found. Starting at 0.");
        }

    }

    public async Task SaveInventoryAsync()
    {
        var saveData = new Dictionary<string, object>
        {
            { "keycards", Keycards }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(saveData);
        Debug.Log($"💾 Saved keycards: {Keycards}");
    }

    public void AddKeycards(int amount)
    {
        Keycards += amount;
        Debug.Log($"➕ Permanently unlocked {amount} keycards. Total: {Keycards}");
        _ = SaveInventoryAsync();
    }
}
