using UnityEngine;
using Unity.Netcode;

public class InteractableCharge : NetworkBehaviour, IInteractableAction
{
    [Header("Battery Settings")]
    [SerializeField] private int maxCharge = 100;

    [Tooltip("Minimum drain per use")]
    [SerializeField] private int minDrain = 1;

    [Tooltip("Maximum drain per use")]
    [SerializeField] private int maxDrain = 15;

    [Header("Light Settings")]
    [SerializeField] private Renderer lightRenderer;
    [SerializeField] private Color chargedColor = Color.green;
    [SerializeField] private Color drainedColor = Color.red;

    private Material lightMatInstance;

    public NetworkVariable<int> currentCharge = new NetworkVariable<int>(
        100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public bool IsDrained => currentCharge.Value <= 0;
    public int CurrentCharge => currentCharge.Value;

    private void Awake()
    {
        if (lightRenderer != null)
        {
            lightMatInstance = lightRenderer.material;
        }
    }

    public override void OnNetworkSpawn()
    {
        currentCharge.OnValueChanged += (_, _) => UpdateLight();
        UpdateLight();
    }

    public void DoAction()
    {
        if (!IsServer || IsDrained) return;

        int drainAmount = GenerateRandomDrain();
        currentCharge.Value = Mathf.Max(0, currentCharge.Value - drainAmount);
        Debug.Log($"{gameObject.name} used battery: -{drainAmount}. Remaining: {currentCharge.Value}");
    }

    private int GenerateRandomDrain()
    {
        float t = Mathf.Pow(Random.value, 2);
        return Mathf.RoundToInt(Mathf.Lerp(minDrain, maxDrain, t));
    }

    public void Recharge(int amount)
    {
        if (!IsServer) return;

        currentCharge.Value = Mathf.Clamp(currentCharge.Value + amount, 0, maxCharge);
        Debug.Log($"{gameObject.name} recharged by {amount}. New charge: {currentCharge.Value}");
    }

    public void FullyRecharge()
    {
        if (!IsServer) return;

        currentCharge.Value = maxCharge;
        Debug.Log($"{gameObject.name} fully recharged.");
    }

    public void FullyDischarge()
    {
        if (!IsServer) return;

        currentCharge.Value = 0;
        Debug.Log($"{gameObject.name} fully discharged.");
    }

    private void UpdateLight()
    {
        if (lightMatInstance != null)
        {
            Color targetColor = IsDrained ? drainedColor : chargedColor;

            // Set base color
            lightMatInstance.color = targetColor;

            // Set emission color
            lightMatInstance.SetColor("_EmissionColor", targetColor);

            // Ensure emission is enabled on the shader
            lightMatInstance.EnableKeyword("_EMISSION");
        }
    }
}
