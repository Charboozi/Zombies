using UnityEngine;
using Unity.Netcode;

public class FlammablePoster : NetworkBehaviour
{
    [Header("Poster States")]
    [SerializeField] private GameObject intactPoster;
    [SerializeField] private GameObject destroyedPoster;

    [Header("Hit Settings")]
    [SerializeField] private float damageThreshold = 1.5f;

    [Header("Linked Button Movement")]
    [SerializeField] private Transform linkedButton;
    [SerializeField] private Transform targetPoint;

    private float damageAccumulated = 0f;

    private NetworkVariable<bool> isDestroyed = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        SetPosterState(true); // fallback
    }

    public override void OnNetworkSpawn()
    {
        isDestroyed.OnValueChanged += (_, newValue) =>
        {
            SetPosterState(!newValue);
        };

        SetPosterState(!isDestroyed.Value);
    }

    public void ApplyBurnDamage(float damageAmount)
    {
        if (!IsServer || isDestroyed.Value) return;

        damageAccumulated += damageAmount;

        if (damageAccumulated >= damageThreshold)
        {
            isDestroyed.Value = true;
            Debug.Log($"🔥 Poster destroyed on server: {name}");
            MoveButtonToTarget();
        }
    }

    private void SetPosterState(bool intact)
    {
        if (intactPoster != null) intactPoster.SetActive(intact);
        if (destroyedPoster != null) destroyedPoster.SetActive(!intact);
    }

    private void MoveButtonToTarget()
    {
        if (linkedButton == null || targetPoint == null)
        {
            Debug.LogWarning("🔧 Linked button or target point is not assigned.");
            return;
        }

        linkedButton.position = targetPoint.position;
        Debug.Log($"🟦 Button {linkedButton.name} moved to {targetPoint.position}");
    }
}
