using UnityEngine;

public class ProceduralArmsAnimation : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobSpeed = 5f;
    public float bobAmount = 0.05f;
    
    [Header("Sway Settings")]
    public float swayAmount = 0.02f;
    
    [Header("Idle (Breathing) Settings")]
    public float idleSpeed = 1f;
    public float idleAmount = 0.01f;
    public float idleSmoothness = 2f;
    
    [Header("Camera-Based Position Sway")]
    public float positionSwayAmount = 0.02f;
    
    [Header("Jump Reaction Settings")]
    public float jumpReactionOffset = 0.1f;
    public float jumpLandSmoothness = 5f;

    // Internal state variables
    private Vector3 originalPosition;
    private float bobTimer = 0f;
    private float idleTimer = 0f;
    private float currentIdleOffset = 0f;
    private Vector3 lastCameraPosition;
    
    // Offsets computed per update
    private Vector3 bobOffset = Vector3.zero;
    private Vector3 swayOffset = Vector3.zero;
    private Vector3 idleOffset = Vector3.zero;
    private Vector3 posSwayOffset = Vector3.zero;
    private Vector3 jumpOffsetEffect = Vector3.zero;

    private Camera playerCamera;

    void Start()
    {
        originalPosition = transform.localPosition;
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("No Main Camera found! Please tag your camera as 'MainCamera'.");
            return;
        }
        lastCameraPosition = playerCamera.transform.position;
    }

    void Update()
    {
        // ===== Bobbing Effect (based on movement input) =====
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        float inputMagnitude = Mathf.Abs(inputX) + Mathf.Abs(inputZ);
        if (inputMagnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            float bobX = Mathf.Sin(bobTimer) * bobAmount;
            float bobY = Mathf.Cos(bobTimer * 2f) * (bobAmount * 0.5f);
            bobOffset = new Vector3(bobX, bobY, 0);
        }
        else
        {
            // Reset bobbing when idle
            bobTimer = 0f;
            bobOffset = Vector3.zero;
        }

        // ===== Sway Effect (based on mouse movement) =====
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;
        swayOffset = new Vector3(-mouseX, -mouseY, 0);

        // ===== Idle Breathing Effect =====
        idleTimer += Time.deltaTime * idleSpeed;
        float targetIdle = Mathf.Sin(idleTimer) * idleAmount;
        currentIdleOffset = Mathf.Lerp(currentIdleOffset, targetIdle, Time.deltaTime * idleSmoothness);
        idleOffset = new Vector3(0, currentIdleOffset, 0);

        // ===== Camera-Based Position Sway (react to camera movement) =====
        Vector3 camDelta = playerCamera.transform.position - lastCameraPosition;
        posSwayOffset = new Vector3(-camDelta.x, -camDelta.y, 0) * positionSwayAmount;

        // ===== Jump Reaction Effect =====
        // If camera moves down quickly, assume a landing impact
        if (playerCamera.transform.position.y < lastCameraPosition.y - 0.05f)
        {
            jumpOffsetEffect = Vector3.Lerp(jumpOffsetEffect, new Vector3(0, -jumpReactionOffset, 0), Time.deltaTime * jumpLandSmoothness);
        }
        else
        {
            jumpOffsetEffect = Vector3.Lerp(jumpOffsetEffect, Vector3.zero, Time.deltaTime * jumpLandSmoothness);
        }

        // ===== Combine all offsets =====
        Vector3 targetOffset = originalPosition + bobOffset + swayOffset + idleOffset + posSwayOffset + jumpOffsetEffect;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, Time.deltaTime * 10f);

        // Update last camera position for next frame
        lastCameraPosition = playerCamera.transform.position;
    }
}
