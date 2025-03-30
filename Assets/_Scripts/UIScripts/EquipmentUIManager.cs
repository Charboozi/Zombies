using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIManager : MonoBehaviour
{
   
    [Tooltip("Prefab for an equipment icon (a UI Image prefab)")]
    public GameObject iconPrefab;

    public void DisplayIcon(Sprite icon)
    {
        if (iconPrefab == null|| icon == null)
            return;
        
        GameObject newIcon = Instantiate(iconPrefab, gameObject.transform);
        Image img = newIcon.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = icon;
        }
    }
}
