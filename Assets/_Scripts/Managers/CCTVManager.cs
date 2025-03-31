using UnityEngine;

public class CCTVManager : MonoBehaviour
{
    private Camera mainCamera;

    [Tooltip("List of CCTV cameras to cycle through in CCTV mode.")]
    public Camera[] cctvCameras;

    [Header("State")]
    [Tooltip("Index of the current active CCTV camera.")]
    public int currentCameraIndex = 0;
    [Tooltip("Is CCTV mode active?")]
    public bool isCCTVActive = false;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        // Disable the Camera component on all CCTV cameras at the start.
        foreach (Camera cam in cctvCameras)
        {
            if (cam != null)
                cam.enabled = false;
        }
    }

    void Update()
    {
        if (isCCTVActive)
        {
            // Cycle cameras with keys (for example, G for next, H for previous).
            if (Input.GetKeyDown(KeyCode.G))
            {
                SwitchToNextCamera();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                SwitchToPreviousCamera();
            }

            // Press O (for example) to exit CCTV mode.
            if (Input.GetKeyDown(KeyCode.O))
            {
                DeactivateCCTV();
            }
        }
    }

    /// <summary>
    /// Activates CCTV mode by disabling the main camera and enabling the first CCTV camera.
    /// </summary>
    public void ActivateCCTV()
    {
        if (isCCTVActive) return;

        isCCTVActive = true;

        // Disable the main camera (only the Camera component).
        if (mainCamera != null)
            mainCamera.enabled = false;

        // Enable the first CCTV camera (only its Camera component).
        if (cctvCameras.Length > 0)
        {
            currentCameraIndex = 0;
            cctvCameras[currentCameraIndex].enabled = true;
        }
    }

    /// <summary>
    /// Exits CCTV mode, disabling all CCTV camera components and re-enabling the main camera.
    /// </summary>
    public void DeactivateCCTV()
    {
        if (!isCCTVActive) return;

        isCCTVActive = false;

        // Disable all CCTV camera components.
        foreach (Camera cam in cctvCameras)
        {
            if (cam != null)
                cam.enabled = false;
        }

        // Re-enable the main camera.
        if (mainCamera != null)
            mainCamera.enabled = true;
    }

    void SwitchToNextCamera()
    {
        if (cctvCameras.Length == 0) return;

        // Disable the current CCTV camera component.
        cctvCameras[currentCameraIndex].enabled = false;
        // Cycle to the next index.
        currentCameraIndex = (currentCameraIndex + 1) % cctvCameras.Length;
        // Enable the new CCTV camera component.
        cctvCameras[currentCameraIndex].enabled = true;
    }

    void SwitchToPreviousCamera()
    {
        if (cctvCameras.Length == 0) return;

        // Disable the current CCTV camera component.
        cctvCameras[currentCameraIndex].enabled = false;
        // Cycle backwards.
        currentCameraIndex = (currentCameraIndex - 1 + cctvCameras.Length) % cctvCameras.Length;
        // Enable the new CCTV camera component.
        cctvCameras[currentCameraIndex].enabled = true;
    }
}
