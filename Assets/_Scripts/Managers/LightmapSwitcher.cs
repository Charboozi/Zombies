using UnityEngine;
using Unity.Netcode;
using System.Collections;

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

    [SerializeField] private GameObject loadingUI;

    private void Awake()
    {
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
            InteractableChargeManager.OnInteractablesReady += OnInteractablesReady;
        }
    }

    private void OnInteractablesReady()
    {
        InteractableChargeManager.OnInteractablesReady -= OnInteractablesReady;
        ApplyLightsOnServerSide();
    }

    private void PrepareLightmaps()
    {
        lightsOnLightmaps = new LightmapData[lightsOnColor.Length];
        for (int i = 0; i < lightsOnColor.Length; i++)
        {
            var data = new LightmapData();
            data.lightmapColor = lightsOnColor[i];
            if (i < lightsOnDirection.Length)
                data.lightmapDir = lightsOnDirection[i];
            lightsOnLightmaps[i] = data;
        }

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

    public void RequestBlackout()
    {
        if (IsServer)
            ApplyBlackoutServerSide();
        else
            RequestBlackoutServerRpc();
    }

    public void RequestLightsOn()
    {
        if (IsServer)
            ApplyLightsOnServerSide();
        else
            RequestLightsOnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestBlackoutServerRpc() => ApplyBlackoutServerSide();

    [ServerRpc(RequireOwnership = false)]
    private void RequestLightsOnServerRpc() => ApplyLightsOnServerSide();

    private void ApplyBlackoutServerSide()
    {
        FullyDischargeAllInteractables();
        ApplyBlackoutClientRpc();
    }

    private void ApplyLightsOnServerSide()
    {
        FullyRechargeAllInteractables();
        ApplyLightsOnClientRpc();
    }

    private void FullyDischargeAllInteractables()
    {
        InteractableChargeManager.Instance?.FullyDischargeAll();
    }

    private void FullyRechargeAllInteractables()
    {
        InteractableChargeManager.Instance?.FullyRechargeAll();
    }

    [ClientRpc]
    private void ApplyBlackoutClientRpc() => ApplyBlackout();

    [ClientRpc]
    private void ApplyLightsOnClientRpc() => ApplyLightsOn();

    private void ApplyBlackout()
    {
        ApplyLightmaps(blackoutLightmaps);
        ApplyReflectionProbes(blackoutReflectionTextures);
        ApplyLightProbes(blackoutLightProbes);
        PlaySound(blackoutSound);
        AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Power_Offline");
        Debug.Log("🕶️ Blackout applied.");
    }

    private void ApplyLightsOn()
    {
        ApplyLightmaps(lightsOnLightmaps);
        ApplyReflectionProbes(lightsOnReflectionTextures);
        ApplyLightProbes(lightsOnLightProbes);
        AnnouncerVoiceManager.Instance.PlayVoiceLineClientRpc("Power_Online");
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
                reflectionProbes[i].RenderProbe();
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
            audioSource.PlayOneShot(clip);
    }

    private IEnumerator Start()
    {
        yield return null;

        if (loadingUI != null)
            loadingUI.SetActive(true);

        yield return PrewarmLightingRoutine();
        ForceFirstSwitch();

        if (loadingUI != null)
            loadingUI.SetActive(false);
    }

    private IEnumerator PrewarmLightingRoutine()
    {
        ApplyLightmaps(blackoutLightmaps);
        ApplyReflectionProbes(blackoutReflectionTextures);
        ApplyLightProbes(blackoutLightProbes);
        yield return new WaitForSeconds(0.25f);

        ApplyLightmaps(lightsOnLightmaps);
        ApplyReflectionProbes(lightsOnReflectionTextures);
        ApplyLightProbes(lightsOnLightProbes);
        yield return new WaitForSeconds(0.25f);

        foreach (var probe in reflectionProbes)
        {
            if (probe != null)
            {
                probe.RenderProbe();
                yield return null;
            }
        }

        Debug.Log("✅ Lightmap prewarm completed.");
    }

    private void ForceFirstSwitch()
    {
        ApplyBlackout();
        ApplyLightsOn();
    }
}
