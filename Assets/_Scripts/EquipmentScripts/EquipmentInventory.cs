using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EquipmentInventory : MonoBehaviour
{
    // Automatically finds all equipment under this GameObject.
    private List<GameObject> allEquipment = new();
    private readonly HashSet<GameObject> equippedItems = new();

    public event Action<GameObject> OnEquipped;

    public EquipmentUIManager uiManager;

    private void Awake()
    {
        // Find all children (can be on different bones) except self.
        allEquipment = GetComponentsInChildren<Transform>(true)
            .Where(t => t.gameObject != gameObject)
            .Select(t => t.gameObject)
            .ToList();
    }

    public void Equip(string equipmentName)
    {
        GameObject equipment = allEquipment.Find(e => e.name == equipmentName);

        if (equipment == null)
        {
            Debug.LogWarning($"Equipment '{equipmentName}' not found among children.");
            return;
        }

        equipment.SetActive(true);
        equippedItems.Add(equipment);
        OnEquipped?.Invoke(equipment);
        
        BaseEquipment baseEquip = equipment.GetComponent<BaseEquipment>();
        if (baseEquip != null && uiManager != null)
        {
            uiManager.DisplayIcon(baseEquip.equipmentIcon);
        }
    }

    public bool HasEquipped(string equipmentName)
    {
        return equippedItems.Any(e => e.name == equipmentName);
    }

    public IReadOnlyCollection<GameObject> EquippedItems => equippedItems;
}
