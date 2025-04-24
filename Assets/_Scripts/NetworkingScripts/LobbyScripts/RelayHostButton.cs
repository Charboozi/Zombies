using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class RelayHostButton : MonoBehaviour
{
    [SerializeField] private TMP_Text codeDisplay;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private void Start()
    {
        createButton.onClick.AddListener(CreateRelayHost);
    }

    private async void CreateRelayHost()
    {
        createButton.interactable = false;
        joinButton.interactable = false; 

        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            if (codeDisplay != null)
                codeDisplay.text = $"{joinCode}";

            Debug.Log("Relay host started with code: " + joinCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("Host creation failed: " + ex.Message);
            createButton.interactable = true;
            joinButton.interactable = true;  // 🔁 Re-enable if something failed
        }
    }
}

