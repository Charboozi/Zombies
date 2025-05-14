using UnityEngine;
using System;

[RequireComponent(typeof(PickupWeapon))]
[RequireComponent(typeof(WeaponController))]
public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }

    public static event Action<Vector2> OnMouseLook;
    public static event Action<Vector2> OnMoveInput;
    public static event Action OnJumpPressed;
    public static event Action OnFirePressed;
    public static event Action OnFireReleased;
    public static event Action<int> OnSwitchWeapon;
    public static event Action<int> OnCycleWeapon;
    public static event Action OnInteractPressed;
    public static event Action OnPausePressed;

    public static bool CanInteract = true;

    private PlayerControls controls;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        controls = new PlayerControls();

        // Bind actions
        controls.Player.Look.performed += ctx =>
        {
            Vector2 look = ctx.ReadValue<Vector2>();

            // Scale down to match old Input.GetAxis behavior
            look *= 0.1f; // or multiply by sensitivity * Time.deltaTime

            OnMouseLook?.Invoke(look);
        };

        controls.Player.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        controls.Player.Move.canceled  += ctx => OnMoveInput?.Invoke(Vector2.zero);

        controls.Player.Jump.performed += ctx => OnJumpPressed?.Invoke();

        controls.Player.Fire.started   += ctx => OnFirePressed?.Invoke();
        controls.Player.Fire.canceled  += ctx => OnFireReleased?.Invoke();

        controls.Player.Interact.performed += ctx => {
            if (CanInteract) OnInteractPressed?.Invoke();
        };

        controls.Player.Pause.performed += ctx => OnPausePressed?.Invoke();

        controls.Player.CycleWeapon.performed += ctx =>
        {
            float scroll = ctx.ReadValue<float>();
            Debug.Log("Scroll: " + scroll);

            if (scroll > 0f)
                OnCycleWeapon?.Invoke(1);
            else if (scroll < 0f)
                OnCycleWeapon?.Invoke(-1);
        };

        // Weapon keys 1–4
        controls.Player.SwitchWeapon1.performed += ctx => OnSwitchWeapon?.Invoke(0);
        controls.Player.SwitchWeapon2.performed += ctx => OnSwitchWeapon?.Invoke(1);
        controls.Player.SwitchWeapon3.performed += ctx => OnSwitchWeapon?.Invoke(2);
        controls.Player.SwitchWeapon4.performed += ctx => OnSwitchWeapon?.Invoke(3);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public Vector2 MoveInput { get; private set; }
    private bool jumpQueued = false;

    public void QueueJump() => jumpQueued = true;

    public bool ConsumeJumpQueued()
    {
        bool temp = jumpQueued;
        jumpQueued = false;
        return temp;
    }
}
