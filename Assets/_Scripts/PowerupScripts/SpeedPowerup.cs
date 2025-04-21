using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class SpeedPowerup : PowerupBase
{
    [Tooltip("Amount of speed to add.")]
    [SerializeField] private float speedBoost = 3f;

    [Tooltip("Duration of the speed boost in seconds.")]
    [SerializeField] private float duration = 20f;

    protected override int GetEffectValue()
    {
        return Mathf.RoundToInt(speedBoost);
    }

    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int _, ClientRpcParams clientRpcParams = default)
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (player == null)
        {
            Debug.LogWarning("SpeedPowerup: No local player found.");
            return;
        }

        var movement = player.GetComponent<NetworkedCharacterMovement>();
        if (movement != null)
        {
            player.GetComponent<MonoBehaviour>().StartCoroutine(ApplySpeedBoost(movement));
        }
        else
        {
            Debug.LogWarning("SpeedPowerup: No movement component found.");
        }
    }

    private IEnumerator ApplySpeedBoost(NetworkedCharacterMovement movement)
    {
        Debug.Log($"⚡ Speed boost applied: +{speedBoost}");
        FadeScreenEffect.Instance.ShowPersistentEffectForDuration(Color.yellow, duration);

        movement.AddBonusSpeed(speedBoost);
        GameObject loopAudio = PlayLoopedEffectSound(duration);

        yield return new WaitForSeconds(duration);

        movement.RemoveBonusSpeed(speedBoost);
        Debug.Log($"⚡ Speed boost ended. Removed: -{speedBoost}");

        if (loopAudio != null)
        {
            Destroy(loopAudio);
        }
    }
}
