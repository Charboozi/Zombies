using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Instantiates an equipment icon and assigns the correct sprite.
/// </summary>
public class EquipmentUI : MonoBehaviour
{
    private GameObject iconPrefab;

    private void Awake()
    {
        iconPrefab = Resources.Load<GameObject>("UI/IconPrefab");
    }

    public void DisplayIcon(Sprite icon)
    {
        if (iconPrefab == null || icon == null)
            return;
        
        GameObject newIcon = Instantiate(iconPrefab, transform);
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = icon;
        }
    }
}
