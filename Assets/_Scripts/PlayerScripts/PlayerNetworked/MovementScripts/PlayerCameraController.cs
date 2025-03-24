using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class FPSCameraController : NetworkBehaviour
{
    public Transform headPosition; // Assign this in the Inspector (Head transform of the player)
    public string[] gameScenes = { "GameScene1", "GameScene2" }; // Add gameplay scene names here

    private float yaw = 0f;
    private float pitch = 0f;
    private Camera playerCamera;
    private bool isGameScene = false;

    [Tooltip("Minimum pitch angle (looking down).")]
    public float minPitch = -80f;
    [Tooltip("Maximum pitch angle (looking up).")]
    public float maxPitch = 80f;
    public float mouseSensitivity = 100f;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded; // Listen for scene changes

        if (IsOwner && IsInGameScene())
        {
            EnableCameraControl();
        }
        else
        {
            enabled = false; // Disable script if not a game scene
        }
    }

    void LateUpdate()
    {
        if (!isGameScene || playerCamera == null || headPosition == null) return;

        // Move the camera to the player's head position
        playerCamera.transform.position = headPosition.position;

        ProcessMouseLook();
    }

    private void ProcessMouseLook()
    {
        // Get mouse input using the old Input system.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Update yaw and pitch.
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        
        // Rotate the player based on yaw only.
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        
        // Set the camera's rotation so that its yaw matches the player's, and its pitch is controlled separately.
        if (playerCamera != null && headPosition != null)
        {
            playerCamera.transform.position = headPosition.position;
            playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private bool IsInGameScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (string scene in gameScenes)
        {
            if (currentScene == scene) return true;
        }
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsOwner && IsInGameScene())
        {
            EnableCameraControl();
        }
    }

    private void EnableCameraControl()
    {
        isGameScene = true;

        // Find the main camera in the scene
        playerCamera = Camera.main;

        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found! Make sure the scene has a Camera tagged 'MainCamera'.");
            return;
        }

        // Lock cursor for FPS movement
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        enabled = true; // Re-enable script when entering a game scene
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Cleanup listener
    }
}
