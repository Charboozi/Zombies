using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ArmorPowerup : PowerupBase
{
    [Tooltip("Amount of temporary armor to apply.")]
    [SerializeField] private int armorBonus = 10;

    [Tooltip("How long the armor lasts (in seconds).")]
    [SerializeField] private float duration = 20f;

    protected override int GetEffectValue()
    {
        return armorBonus;
    }

    [ClientRpc]
    protected override void ApplyPowerupClientRpc(int _, ClientRpcParams clientRpcParams = default)
    {
        var player = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (player == null)
        {
            Debug.LogWarning("ArmorPowerup: No local player found.");
            return;
        }

        var proxy = player.GetComponent<HealthProxy>();
        if (proxy != null)
        {
            StartCoroutine(ApplyTemporaryArmor(proxy)); // ✅ run locally
        }
        else
        {
            Debug.LogWarning("ArmorPowerup: No HealthProxy found on player.");
        }
    }

    private IEnumerator ApplyTemporaryArmor(HealthProxy proxy)
    {
        Debug.Log($"🛡️ Temporary armor applied: +{armorBonus}");
        proxy.AddArmor(armorBonus);

        PersistentScreenTint.Instance.SetPersistentTintForDuration(
            new Color(0.2f, 0.8f, 0.2f), duration, 0.05f
        );

        GameObject loopAudio = PlayLoopedEffectSound(duration);

        yield return new WaitForSeconds(duration);

        proxy.RemoveArmor(armorBonus);
        Debug.Log($"🛡️ Temporary armor expired: -{armorBonus}");

        if (loopAudio != null)
        {
            Destroy(loopAudio);
        }
    }
}
