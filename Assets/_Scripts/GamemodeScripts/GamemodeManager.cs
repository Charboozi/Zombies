using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    // Publicly accessible flag
    public bool IsPvPMode { get; private set; }

    private void Awake()
    {
        // Ensure singleton and persistence across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Set whether the game is in PvP mode or not.
    /// </summary>
    public void SetPvPMode(bool isPvP)
    {
        IsPvPMode = isPvP;
        Debug.Log($"🔁 PvP mode set to: {isPvP}");
    }
}
