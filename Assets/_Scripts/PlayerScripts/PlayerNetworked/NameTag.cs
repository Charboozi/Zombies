using UnityEngine;
using Unity.Netcode;
using TMPro;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNameTag : NetworkBehaviour
{
    [Header("Assign your world-space TextMeshPro here")]
    [SerializeField] private TMP_Text nameText;
    
    Transform cameraTransform;

    public override void OnNetworkSpawn()
    {
        // Cache the camera
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        // Set initial text to the client ID
        nameText.text = OwnerClientId.ToString();

        // If you later change the ID or name, you could hook up a callback here.
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;
        // Make the name-tag face the camera
        Vector3 direction = transform.position - cameraTransform.position;
        // Keep its upright orientation
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        nameText.transform.parent.rotation = lookRotation;
    }
}
