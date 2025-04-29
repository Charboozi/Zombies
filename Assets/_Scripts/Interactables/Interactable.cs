using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NetworkObject))]
public class Interactable : NetworkBehaviour
{
    [Header("Interact Settings")]
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private string interactText = "Use Terminal";

    [Header("Blackout Settings")]
    [SerializeField, Range(0f, 1f)] private float blackoutChanceOnUse = 0.02f; // 2% chance

    [Header("Access Settings")]
    [SerializeField] private bool requiresKeycard = true;
    [SerializeField] private int keycardsRequired = 1;

    public NetworkVariable<bool> isCoolingDown = new NetworkVariable<bool>(false);

    public string GetInteractText()
    {
        return isCoolingDown.Value ? "Cooling down..." : interactText;
    }

    public void Interact()
    {
        // Check if this object allows interaction (dynamic check)
        var checker = GetComponent<ICheckIfInteractable>();
        if (checker != null && !checker.CanCurrentlyInteract())
        {
            Debug.Log("❌ Interaction checker said NO. Blocking.");
            return;
        }

        if (isCoolingDown.Value) return;

        var battery = GetComponent<InteractableCharge>();

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
        else if (requiresKeycard)
        {
            var manager = ConsumableManager.Instance;
            if (manager == null)
            {
                Debug.Log("🔒 Keycard manager not found.");
                return;
            }

            int keycardsConsumed = 0;
            for (int i = 0; i < keycardsRequired; i++)
            {
                if (manager.Use("Keycard"))
                    keycardsConsumed++;
                else
                    break;
            }

            if (keycardsConsumed < keycardsRequired)
            {
                Debug.Log($"🔒 Not enough keycards. Needed {keycardsRequired}, but only had {keycardsConsumed}.");
                return;
            }

            Debug.Log($"🔓 Used {keycardsConsumed} keycard(s) to access interactable.");
        }

        // Client-only logic (runs immediately)
        var localOnly = GetComponent<IClientOnlyAction>();
        if (localOnly != null)
            localOnly.DoClientAction();

        // Server-only logic
        var broadcast = GetComponent<IBroadcastClientAction>();
        if (broadcast != null)
            RequestBroadcastClientRpcServerRpc();

        RequestInteractServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isCoolingDown.Value) return;

        ulong interactorId = rpcParams.Receive.SenderClientId;

        var actions = GetComponents<IInteractableAction>();
        foreach (var action in actions)
        {
            if (action is IInteractorAwareAction aware)
                aware.DoAction(interactorId);
            else
                action.DoAction();
        }

        TryTriggerBlackout();

        PlayInteractionSoundClientRpc();

        isCoolingDown.Value = true;
        StartCoroutine(CooldownRoutine());
    }

    private void TryTriggerBlackout()
    {
        if (Random.value <= blackoutChanceOnUse)
        {
            Debug.Log("⚡ Random blackout triggered!");
            LightmapSwitcher.Instance?.RequestBlackout();
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
        var broadcast = GetComponent<IBroadcastClientAction>();
        broadcast?.DoAllClientsAction();
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
