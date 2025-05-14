using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class FPSCameraController : NetworkBehaviour
{
    [Header("References")]
    public Transform headPosition; // Assigned in inspector (child of player root)
    public CameraRecoilController cameraRecoil; // Assigned in inspector (on child of head)
    public string[] gameScenes = { "GameScene1", "GameScene2" };

    private Camera playerCamera;
    private bool isGameScene = false;

    [Header("Look Settings")]
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public float mouseSensitivity = 100f;

    [Header("Camera Offset Settings")]
    public float cameraLerpSpeed = 5f;

    private float yaw = 0f;
    private float pitch = 0f;

    private Vector3 currentCameraOffset = Vector3.zero;
    private Vector3 targetCameraOffset = Vector3.zero;

    private Quaternion currentCameraTilt = Quaternion.identity;
    private Quaternion targetCameraTilt = Quaternion.identity;

    private EntityHealth entityHealth;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        entityHealth = GetComponent<EntityHealth>();

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

            if (entityHealth.isDowned.Value)
                SetDownedCameraTarget();
        }
    }

    private void OnDownedStateChanged(bool previous, bool current)
    {
        if (current)
            SetDownedCameraTarget();
        else
            SetNormalCameraTarget();
    }

    private void SetDownedCameraTarget()
    {
        targetCameraOffset = Vector3.down * 1f;
        targetCameraTilt = Quaternion.Euler(20f, 0f, 0f);
    }

    private void SetNormalCameraTarget()
    {
        targetCameraOffset = Vector3.zero;
        targetCameraTilt = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (!isGameScene || playerCamera == null || headPosition == null || cameraRecoil == null)
            return;

        // Smooth tilt & offset when downed
        currentCameraOffset = Vector3.Lerp(currentCameraOffset, targetCameraOffset, Time.deltaTime * cameraLerpSpeed);
        currentCameraTilt = Quaternion.Lerp(currentCameraTilt, targetCameraTilt, Time.deltaTime * cameraLerpSpeed);

        // Apply camera position
        playerCamera.transform.position = cameraRecoil.transform.position + currentCameraOffset;

        // Apply full rotation: pitch from recoil object + tilt
        playerCamera.transform.rotation = cameraRecoil.transform.rotation * currentCameraTilt;
    }

    private void Update()
    {
        if (!isGameScene || !IsOwner) return;

        ProcessMouseLook();
    }

    private void ProcessMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // YAW → player body
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Get recoil rotation from controller
        Vector2 recoil = cameraRecoil != null ? cameraRecoil.GetCurrentRecoil() : Vector2.zero;

        // PITCH + RECOIL applied to recoil transform
        cameraRecoil.transform.localRotation = Quaternion.Euler(pitch - recoil.y, recoil.x, 0f);
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

        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("❌ No Main Camera found!");
            return;
        }

        if (cameraRecoil == null)
        {
            Debug.LogError("❌ CameraRecoilController reference not assigned!");
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        enabled = true;
    }

    public override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (entityHealth != null)
            entityHealth.isDowned.OnValueChanged -= OnDownedStateChanged;
    }
}
