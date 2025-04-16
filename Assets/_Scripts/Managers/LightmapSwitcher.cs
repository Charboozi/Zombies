using UnityEngine;
using Unity.Netcode;

public class LightmapSwitcher : NetworkBehaviour
{
    public static LightmapSwitcher Instance { get; private set; }

    [Header("Lights On Lightmaps")]
    public Texture2D[] lightsOnColor;
    public Texture2D[] lightsOnDirection;

    [Header("Blackout Lightmaps")]
    public Texture2D[] blackoutColor;
    public Texture2D[] blackoutDirection;

    [Header("Reflection Probes")]
    public ReflectionProbe[] reflectionProbes;
    public Texture[] lightsOnReflectionTextures;
    public Texture[] blackoutReflectionTextures;

    [Header("Light Probes")]
    public LightProbes lightsOnLightProbes;
    public LightProbes blackoutLightProbes;

    private LightmapData[] lightsOnLightmaps;
    private LightmapData[] blackoutLightmaps;

    [Header("Audio")]
    [SerializeField] private AudioClip blackoutSound;
    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();

        PrepareLightmaps();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RequestLightsOn();
        }
    }

    private void PrepareLightmaps()
    {
        // Prepare Lights On Lightmaps
        lightsOnLightmaps = new LightmapData[lightsOnColor.Length];
        for (int i = 0; i < lightsOnColor.Length; i++)
        {
            var data = new LightmapData();
            data.lightmapColor = lightsOnColor[i];
            if (i < lightsOnDirection.Length)
                data.lightmapDir = lightsOnDirection[i];
            lightsOnLightmaps[i] = data;
        }

        // Prepare Blackout Lightmaps
        blackoutLightmaps = new LightmapData[blackoutColor.Length];
        for (int i = 0; i < blackoutColor.Length; i++)
        {
            var data = new LightmapData();
            data.lightmapColor = blackoutColor[i];
            if (i < blackoutDirection.Length)
                data.lightmapDir = blackoutDirection[i];
            blackoutLightmaps[i] = data;
        }
    }

    // === PUBLIC METHODS ===

    public void RequestBlackout()
    {
        if (IsServer)
        {
            ApplyBlackoutClientRpc();
        }
        else
        {
            RequestBlackoutServerRpc();
        }
    }

    public void RequestLightsOn()
    {
        if (IsServer)
        {
            ApplyLightsOnClientRpc();
        }
        else
        {
            RequestLightsOnServerRpc();
        }
    }

    // === SERVER RPCS ===

    [ServerRpc(RequireOwnership = false)]
    private void RequestBlackoutServerRpc(ServerRpcParams rpcParams = default)
    {
        ApplyBlackoutClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestLightsOnServerRpc(ServerRpcParams rpcParams = default)
    {
        ApplyLightsOnClientRpc();
    }

    // === CLIENT RPCS ===

    [ClientRpc]
    private void ApplyBlackoutClientRpc()
    {
        ApplyBlackout();
    }

    [ClientRpc]
    private void ApplyLightsOnClientRpc()
    {
        ApplyLightsOn();
    }

    // === LOCAL APPLICATION ===

    private void ApplyBlackout()
    {
        ApplyLightmaps(blackoutLightmaps);
        ApplyReflectionProbes(blackoutReflectionTextures);
        ApplyLightProbes(blackoutLightProbes);

        // Discharge interactables
        if (InteractableChargeManager.Instance != null)
        {
            InteractableChargeManager.Instance.FullyDischargeAll();
        }

        PlaySound(blackoutSound);

        Debug.Log("🕶️ Blackout applied.");
    }

    private void ApplyLightsOn()
    {
        ApplyLightmaps(lightsOnLightmaps);
        ApplyReflectionProbes(lightsOnReflectionTextures);
        ApplyLightProbes(lightsOnLightProbes);

        // Recharge interactables
        if (InteractableChargeManager.Instance != null)
        {
            InteractableChargeManager.Instance.FullyRechargeAll();
        }

        AnnouncerVoiceManager.Instance.PlayVoiceLine("Power_Online");
        Debug.Log("💡 Lights On applied.");
    }

    private void ApplyLightmaps(LightmapData[] lightmaps)
    {
        LightmapSettings.lightmaps = lightmaps;
    }

    private void ApplyReflectionProbes(Texture[] textures)
    {
        for (int i = 0; i < reflectionProbes.Length; i++)
        {
            if (i < textures.Length)
            {
                reflectionProbes[i].customBakedTexture = textures[i];
                reflectionProbes[i].RenderProbe(); // Optional refresh
            }
        }
    }

    private void ApplyLightProbes(LightProbes probes)
    {
        LightmapSettings.lightProbes = probes;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
            AnnouncerVoiceManager.Instance.PlayVoiceLine("Power_Offline");
        }
    }

}
