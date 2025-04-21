using UnityEngine;
using Unity.Netcode;

public class Vest : BaseEquipment
{
    [SerializeField] private int armorBonus = 2;
    public int ArmorBonus => armorBonus;

    private bool effectApplied = false;

    private void OnEnable()
    {
        if (!effectApplied)
        {
            var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (localPlayerObj != null)
            {
                var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
                if (healthProxy != null)
                {
                    healthProxy.AddArmor(armorBonus);
                    effectApplied = true;
                    Debug.Log($"{gameObject.name} applied its armor bonus: +{armorBonus}");
                }
            }
        }
    }

    private void OnDisable()
    {
        // If we never applied the effect, or we aren't on a running client, do nothing:
        if (!effectApplied 
        || NetworkManager.Singleton == null 
        || !NetworkManager.Singleton.IsClient)
            return;

        var localPlayerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObj == null) return;

        var healthProxy = localPlayerObj.GetComponent<HealthProxy>();
        if (healthProxy == null) return;

        healthProxy.RemoveArmor(armorBonus);
        effectApplied = false;
        Debug.Log($"{gameObject.name} removed its armor bonus: -{armorBonus}");
    }
}
