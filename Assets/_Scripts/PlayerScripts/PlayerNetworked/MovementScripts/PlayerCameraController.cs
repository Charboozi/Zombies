using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class FPSCameraController : NetworkBehaviour
{
    [Header("References")]
    public Transform headPosition; // Empty GameObject attached to (or marking) the player's head
    public string[] gameScenes = { "GameScene1", "GameScene2" }; // Names of your gameplay scenes

    private float yaw = 0f;
    private float pitch = 0f;
    private Camera playerCamera;
    private bool isGameScene = false;

    [Header("Look Settings")]
    [Tooltip("Minimum pitch angle (looking down).")]
    public float minPitch = -80f;
    [Tooltip("Maximum pitch angle (looking up).")]
    public float maxPitch = 80f;
    public float mouseSensitivity = 100f;

    [Header("Camera Offset Settings")]
    [Tooltip("Speed at which the camera offset and tilt interpolate.")]
    public float cameraLerpSpeed = 5f;

    // Instead of modifying headPosition, we maintain a local offset (and rotation tilt) for the camera.
    private Vector3 currentCameraOffset = Vector3.zero; // Runtime offset (changes over time)
    private Vector3 targetCameraOffset = Vector3.zero;  // Where the offset should be (downed or normal)

    private Quaternion currentCameraTilt = Quaternion.identity; // Runtime tilt of the camera
    private Quaternion targetCameraTilt = Quaternion.identity;  // Target tilt (set when downed)

    private EntityHealth entityHealth;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        entityHealth = GetComponent<EntityHealth>();

        // Initialize our offset/tilt values.
        currentCameraOffset = Vector3.zero;
        targetCameraOffset = Vector3.zero;
        currentCameraTilt = Quaternion.identity;
        targetCameraTilt = Quaternion.identity;

        if (IsOwner && IsInGameScene())
        {
            EnableCameraControl();
        }
        else
        {
            enabled = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        entityHealth = GetComponent<EntityHealth>();
        if (entityHealth != null)
        {
            entityHealth.isDowned.OnValueChanged += OnDownedStateChanged;
            // For late joiners: if already downed, set target offset immediately.
            if (entityHealth.isDowned.Value)
            {
                SetDownedCameraTarget();
            }
        }
    }

    private void OnDownedStateChanged(bool previous, bool current)
    {
        if (current) // Player is downed
        {
            SetDownedCameraTarget();
        }
        else // Player is revived
        {
            SetNormalCameraTarget();
        }
    }

    // When downed, we want the camera to lower by 1 unit and optionally apply a slight tilt.
    private void SetDownedCameraTarget()
    {
        targetCameraOffset = Vector3.down * 1f;  
        targetCameraTilt = Quaternion.Euler(20f, 0f, 0f); // Apply a 20° tilt downward (optional)
    }

    // When revived, we want no offset or tilt.
    private void SetNormalCameraTarget()
    {
        targetCameraOffset = Vector3.zero;
        targetCameraTilt = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (!isGameScene || playerCamera == null || headPosition == null)
            return;

        // Smoothly Lerp the camera offset and tilt toward the target values
        currentCameraOffset = Vector3.Lerp(currentCameraOffset, targetCameraOffset, Time.deltaTime * cameraLerpSpeed);
        currentCameraTilt = Quaternion.Lerp(currentCameraTilt, targetCameraTilt, Time.deltaTime * cameraLerpSpeed);

        // The desired camera position is the head's current position plus our offset.
        playerCamera.transform.position = headPosition.position + currentCameraOffset;

        // Process mouse look to update yaw/pitch and rotate the camera accordingly.
        ProcessMouseLook();

        // Then, apply our tilt (multiplicatively) on top of the base rotation.
        playerCamera.transform.rotation = playerCamera.transform.rotation * currentCameraTilt;
    }

    private void ProcessMouseLook()
    {
        // Get mouse input.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Rotate the player's body (yaw only).
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        
        // Set the base camera rotation (pitch + yaw). We'll apply our additional tilt on top later.
        if (playerCamera != null)
        {
            playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private bool IsInGameScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        foreach (string scene in gameScenes)
        {
            if (currentScene == scene)
                return true;
        }
        return false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsOwner && IsInGameScene())
            EnableCameraControl();
    }

    private void EnableCameraControl()
    {
        isGameScene = true;

        // Find the main camera.
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found! Make sure the scene has a camera tagged 'MainCamera'.");
            return;
        }

        // Lock the cursor for typical FPS control.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        enabled = true;
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (entityHealth != null)
        {
            entityHealth.isDowned.OnValueChanged -= OnDownedStateChanged;
        }
    }
}
