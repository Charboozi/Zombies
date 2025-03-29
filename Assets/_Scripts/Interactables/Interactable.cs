using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class Interactable : NetworkBehaviour
{
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private string interactText = "Use Terminal";

    public NetworkVariable<bool> isCoolingDown = new NetworkVariable<bool>(false);

    public string GetInteractText()
    {
        return isCoolingDown.Value ? "Cooling down..." : interactText;
    }

    public void Interact()
    {
        // Only the owner (client) should call this, and it will request the server to handle the logic
        if (isCoolingDown.Value) return;

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

        // Start server-side cooldown
        isCoolingDown.Value = true;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isCoolingDown.Value = false;
    }
}
