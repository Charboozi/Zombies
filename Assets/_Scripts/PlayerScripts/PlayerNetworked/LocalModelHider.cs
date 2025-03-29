using UnityEngine;
using Unity.Netcode;

public class LocalModelHider : NetworkBehaviour
{
    [SerializeField] private GameObject modelRoot;

    void Start()
    {
        if (!IsOwner || modelRoot == null) return;

        foreach (var skinnedMesh in modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            skinnedMesh.enabled = false;

        foreach (var mesh in modelRoot.GetComponentsInChildren<MeshRenderer>(true))
            mesh.enabled = false;
    }
}
