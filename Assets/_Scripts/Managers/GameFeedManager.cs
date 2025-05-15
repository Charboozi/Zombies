using UnityEngine;
using Unity.Netcode;

public class GameFeedManager : NetworkBehaviour
{
    public static GameFeedManager Instance;

    [SerializeField] private Transform feedPanel;
    [SerializeField] private FeedEntry feedEntryPrefab;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PostFeedMessageServerRpc("Test feed message from F key.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PostFeedMessageServerRpc(string message)
    {
        Debug.Log($"📨 [ServerRpc] PostFeedMessageServerRpc called with message: {message}");
        PostFeedMessageClientRpc(message);
    }

    [ClientRpc]
    public void PostFeedMessageClientRpc(string message)
    {
        Debug.Log($"📩 [ClientRpc] PostFeedMessageClientRpc received: {message}");

        var entry = Instantiate(feedEntryPrefab, feedPanel);
        entry.SetMessage(message);
    }
}
