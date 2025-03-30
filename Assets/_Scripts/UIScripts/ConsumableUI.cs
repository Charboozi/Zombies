using TMPro;
using UnityEngine;

public class ConsumableUI : MonoBehaviour
{
    [SerializeField] private string consumableType;
    [SerializeField] private ConsumableManager manager;
    [SerializeField] private TextMeshProUGUI text;

    private void Start()
    {
        manager.OnConsumableChanged.AddListener(OnChanged);
        UpdateUI(manager.Get(consumableType));
    }

    private void OnChanged(string type, int count)
    {
        if (type == consumableType)
            UpdateUI(count);
    }

    private void UpdateUI(int count)
    {
        text.text = $"{count}";
    }
}
