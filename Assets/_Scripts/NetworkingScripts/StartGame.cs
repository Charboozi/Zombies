using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [Tooltip("Exact name of your game scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only host can start the game.");
            return;
        }

        NetworkManager.Singleton.SceneManager.LoadScene(
            gameSceneName,
            LoadSceneMode.Single
        );
    }
}
