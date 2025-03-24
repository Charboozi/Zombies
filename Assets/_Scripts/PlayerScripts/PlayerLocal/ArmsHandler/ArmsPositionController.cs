using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class ArmsPositionController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 basePosition;
    private List<IArmsOffsetProvider> offsetProviders = new();
    private Transform weaponTransform;

    void Awake()
    {
        weaponTransform = transform;
        basePosition = weaponTransform.localPosition;

        // Find all components that contribute to animation
        offsetProviders.AddRange(GetComponents<IArmsOffsetProvider>());
    }

    void Update()
    {
        Vector3 finalOffset = Vector3.zero;
        foreach (var provider in offsetProviders)
            finalOffset += provider.GetOffset();

        Vector3 targetPosition = basePosition + finalOffset;
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
