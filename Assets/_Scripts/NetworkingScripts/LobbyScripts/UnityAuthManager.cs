using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;
using Steamworks;

public class UnityAuthManager : MonoBehaviour
{
    public static UnityAuthManager Instance;

    public bool IsAuthenticated => AuthenticationService.Instance.IsSignedIn;

    [SerializeField] private TMP_Text playerIdText;

    private async void Awake()
    {
        // Make this persist across scenes if needed
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await AuthenticateAsync();
    }

    public async Task AuthenticateAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"✅ Signed in anonymously as PlayerID: {AuthenticationService.Instance.PlayerId}");
            }

            if (PlayerInventoryManager.Instance != null)
            {
                await PlayerInventoryManager.Instance.LoadKeycardsAsync();
                await PlayerInventoryManager.Instance.LoadUnlockedWeaponsAsync();
            }

            // ✅ SAFE POINT: Now we are signed in — initialize currency
            if (CurrencyManager.Instance != null)
            {
                await CurrencyManager.Instance.InitAsync();
                Debug.Log($"💰 Coins after load: {CurrencyManager.Instance.Coins}");
            }

            // Optional: show Steam name in UI
            if (SteamManager.Initialized && playerIdText != null)
            {
                playerIdText.text = SteamFriends.GetPersonaName();
            }
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Unity Authentication failed: " + ex.Message);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Unity Services initialization failed: " + ex.Message);
        }
    }

}