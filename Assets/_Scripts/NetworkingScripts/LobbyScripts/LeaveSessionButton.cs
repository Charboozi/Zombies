using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(Button))]
public class LeaveSessionButton : MonoBehaviour
{
    private Button button;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LeaveSession);
    }

    private void LeaveSession()
    {
        if (!NetworkManager.Singleton) return;

        joinButton.interactable = true;
        createButton.interactable = true;

        PlayerListUI.Instance?.ClearList();

        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
