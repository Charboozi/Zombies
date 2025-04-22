using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverSceneManager : MonoBehaviour
{
    [SerializeField] private float returnToLobbyDelay = 10f;
    [SerializeField] private string lobbySceneName = "MainMenu"; // Change to your actual scene name

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"🟥 Game Over — returning to lobby in {returnToLobbyDelay} seconds...");
        StartCoroutine(ReturnToLobbyAfterDelay());
    }

    private IEnumerator ReturnToLobbyAfterDelay()
    {
        yield return new WaitForSeconds(returnToLobbyDelay);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        }
    }
}
