using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    private Button startButton;

    void Start()
    {
        startButton = GetComponent<Button>();
        startButton.onClick.AddListener(ChangeScene);
    }

    private void ChangeScene()
    {
        if (NetworkManager.Singleton.IsServer) // Only the server/host can change scenes
        {
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("Only the host can start the game.");
        }
    }
}
