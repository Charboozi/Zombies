using UnityEngine;

public class ReturnToMainMenuAfterGameOver : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject menuPanel;

    private void Start()
    {
        if (GameOverSceneManager.OpenLobbyAfterLoad)
        {
            lobbyPanel.SetActive(true);
            menuPanel.SetActive(false);
            GameOverSceneManager.OpenLobbyAfterLoad = false; // reset flag
        }
    }

}
