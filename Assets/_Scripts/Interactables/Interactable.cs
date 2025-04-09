using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Interactable : NetworkBehaviour
{
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private string interactText = "Use Terminal";

    [Header("Blackout Settings")]
    [SerializeField, Range(0f, 1f)] private float blackoutChanceOnUse = 0.02f; // 2% chance

    public NetworkVariable<bool> isCoolingDown = new NetworkVariable<bool>(false);

    public string GetInteractText()
    {
        return isCoolingDown.Value ? "Cooling down..." : interactText;
    }

    public void Interact()
    {
        if (isCoolingDown.Value) return;

        var battery = GetComponent<InteractableCharge>();
        
        // 🔋 Interactables with battery
        if (battery != null)
        {
            if (battery.IsDrained)
            {
                var manager = ConsumableManager.Instance;
                if (manager == null || !manager.Use("Keycard"))
                {
                    Debug.Log("🔒 Battery is drained and no keycard available.");
                    return;
                }

                Debug.Log("🔓 Used keycard to override battery lock.");
            }
        }

        // 👇 Special case: if the object wants to run client-side logic only on local client
        var localOnly = GetComponent<IClientOnlyAction>();
        if (localOnly != null)
        {
            localOnly.DoClientAction();
        }

        // 👇 Everyone triggers this, but server will broadcast it
        var broadcast = GetComponent<IBroadcastClientAction>();
        if (broadcast != null)
        {
            RequestBroadcastClientRpcServerRpc();
        }
        RequestInteractServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isCoolingDown.Value) return;

        // Execute all interaction actions (e.g. spawn weapon)
        var actions = GetComponents<IInteractableAction>();
        foreach (var action in actions)
        {
            action.DoAction(); // already server-side
        }

        TryTriggerBlackout();

        PlayInteractionSoundClientRpc();

        // Start server-side cooldown
        isCoolingDown.Value = true;
        StartCoroutine(CooldownRoutine());
    }

    private void TryTriggerBlackout()
    {
        if (Random.value <= blackoutChanceOnUse)
        {
            Debug.Log("⚡ Random blackout triggered by interactable use!");

            if (LightmapSwitcher.Instance != null)
            {
                LightmapSwitcher.Instance.RequestBlackout();
            }
            else
            {
                Debug.LogWarning("⚠️ LightmapSwitcher not found in scene.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBroadcastClientRpcServerRpc(ServerRpcParams rpcParams = default)
    {
        RunAllClientsActionClientRpc();
    }

    [ClientRpc]
    private void RunAllClientsActionClientRpc()
    {
        var allClientsAction = GetComponent<IBroadcastClientAction>();
        if (allClientsAction != null)
        {
            allClientsAction.DoAllClientsAction();
        }
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown.Value = false;
    }

    [ClientRpc]
    private void PlayInteractionSoundClientRpc()
    {
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
