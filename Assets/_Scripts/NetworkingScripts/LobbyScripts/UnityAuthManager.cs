using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using TMPro;

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
                Debug.Log($"Signed in anonymously as PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            
            playerIdText.text = AuthenticationService.Instance.PlayerId;
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
