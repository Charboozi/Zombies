using UnityEngine;
using Unity.Netcode;

public class Shoes : BaseEquipment
{
    [SerializeField] private float speedBonus = 2f; // Extra speed when equipped

    private bool effectApplied = false;

    private void OnEnable()
    {
        ApplySpeedBonus();
    }

    private void ApplySpeedBonus()
    {
        if (effectApplied) return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj != null)
        {
            var movement = localPlayerObj.GetComponent<NetworkedCharacterMovement>();
            if (movement != null)
            {
                movement.moveSpeed += speedBonus;
                effectApplied = true;
                Debug.Log($"{gameObject.name} applied speed bonus: +{speedBonus}");
            }
        }
    }
}
