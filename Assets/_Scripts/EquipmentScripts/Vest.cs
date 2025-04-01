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
            // Find the local player's network object.
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
}
