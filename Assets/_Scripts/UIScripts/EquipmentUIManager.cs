using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EquipmentUI : MonoBehaviour
{
    private GameObject iconPrefab;
    private Dictionary<Sprite, GameObject> activeIcons = new();

    private void Awake()
    {
        iconPrefab = Resources.Load<GameObject>("UI/IconPrefab");
    }

    public void DisplayIcon(Sprite icon)
    {
        if (iconPrefab == null || icon == null)
            return;

        // Prevent duplicate icons for the same sprite
        if (activeIcons.ContainsKey(icon))
            return;

        GameObject newIcon = Instantiate(iconPrefab, transform);
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = icon;
        }

        activeIcons[icon] = newIcon;
    }

    public void HideIcon(Sprite icon)
    {
        if (icon == null) return;

        if (activeIcons.TryGetValue(icon, out GameObject iconObj))
        {
            Destroy(iconObj);
            activeIcons.Remove(icon);
        }
    }
}
