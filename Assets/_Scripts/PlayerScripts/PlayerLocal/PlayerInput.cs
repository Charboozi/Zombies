using UnityEngine;
using System;

[RequireComponent(typeof(PickupWeapon))]
[RequireComponent(typeof(WeaponController))]
public class PlayerInput : MonoBehaviour
{
    // Movement
    public static event Action<Vector2> OnMouseLook;
    public static event Action<Vector2> OnMoveInput;
    public static event Action OnJumpPressed;

    // Combat
    public static event Action OnFirePressed;
    public static event Action OnFireReleased;

    // Weapons
    public static event Action<int> OnSwitchWeapon;
    public static event Action<int> OnCycleWeapon; // -1 = previous, +1 = next

    // Misc
    public static event Action OnInteractPressed;
    public static event Action OnPausePressed;

    [Header("Controller Bindings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string jumpButton = "Jump";
    public string fireButton = "Fire1";
    public string interactButton = "Interact";
    public string pauseButton = "Pause";
    

    private void Update()
    {
        // Pause
        if (Input.GetButtonDown(pauseButton))
            OnPausePressed?.Invoke();

        if (PauseManager.IsPaused)
        return;

        // Mouse Look
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector2 lookDelta = new Vector2(mouseX, mouseY);
        OnMouseLook?.Invoke(lookDelta);

        // Movement
        float h = Input.GetAxis(horizontalAxis);
        float v = Input.GetAxis(verticalAxis);
        OnMoveInput?.Invoke(new Vector2(h, v));

        // Jump
        if (Input.GetButtonDown(jumpButton))
            OnJumpPressed?.Invoke();

        // Fire
        if (Input.GetButtonDown(fireButton))
            OnFirePressed?.Invoke();
        if (Input.GetButtonUp(fireButton))
            OnFireReleased?.Invoke();

        // Interact
        if (Input.GetButtonDown(interactButton))
            OnInteractPressed?.Invoke(); 

        // Weapon switching: Number keys 1–3
        for (int i = 1; i <= 4; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                OnSwitchWeapon?.Invoke(i - 1);
            }
        }

        // Weapon cycling: mouse scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            OnCycleWeapon?.Invoke(1);
        else if (scroll < 0f)
            OnCycleWeapon?.Invoke(-1);
    }
}
