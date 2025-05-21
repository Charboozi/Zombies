using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private TMP_Text fovValueLabel;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown aaDropdown;

    [Header("Target References")]
    [SerializeField] private Camera mainCamera;

    private Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;

        SetupResolutions();
        SetupQualityDropdown();
        SetupAntiAliasingDropdown();
        LoadSettings();

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
        if (fovSlider != null)
            fovSlider.onValueChanged.AddListener(SetFOV);
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(SetVSync);
        if (aaDropdown != null)
            aaDropdown.onValueChanged.AddListener(SetAntiAliasing);
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        List<string> options = new();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        savedIndex = Mathf.Clamp(savedIndex, 0, resolutions.Length - 1);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupQualityDropdown()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

        int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = Mathf.Clamp(savedQuality, 0, QualitySettings.names.Length - 1);
        qualityDropdown.RefreshShownValue();
    }

    private void SetupAntiAliasingDropdown()
    {
        if (aaDropdown == null) return;

        aaDropdown.ClearOptions();
        aaDropdown.AddOptions(new List<string> { "Off", "2x", "4x", "8x" });
    }

    private void LoadSettings()
    {
        if (resolutions == null || resolutions.Length == 0)
            resolutions = Screen.resolutions;

        // Resolution & fullscreen
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        resolutionIndex = Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1);
        Resolution res = resolutions[resolutionIndex];
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        Screen.SetResolution(res.width, res.height, isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        Screen.fullScreen = isFullscreen;

        if (resolutionDropdown != null)
            resolutionDropdown.value = resolutionIndex;
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = isFullscreen;

        // Quality
        int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        quality = Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(quality);
        if (qualityDropdown != null)
            qualityDropdown.value = quality;

        // FOV
        float fov = PlayerPrefs.GetFloat("FOV", 60f);
        fov = Mathf.Clamp(fov, 50f, 90f);
        if (mainCamera != null)
            mainCamera.fieldOfView = fov;
        if (fovSlider != null)
            fovSlider.value = fov;
        if (fovValueLabel != null)
            fovValueLabel.text = Mathf.RoundToInt(fov).ToString();

        // VSync
        bool vsync = PlayerPrefs.GetInt("VSync", 1) == 1;
        QualitySettings.vSyncCount = vsync ? 1 : 0;
        if (vsyncToggle != null)
            vsyncToggle.isOn = vsync;

        // AA
        int aaLevelIndex = PlayerPrefs.GetInt("AntiAliasing", 0);
        int[] aaLevels = { 0, 2, 4, 8 };
        aaLevelIndex = Mathf.Clamp(aaLevelIndex, 0, aaLevels.Length - 1);
        QualitySettings.antiAliasing = aaLevels[aaLevelIndex];
        if (aaDropdown != null)
            aaDropdown.value = aaLevelIndex;
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || index >= resolutions.Length) return;

        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    public void SetFOV(float fov)
    {
        float clampedFOV = Mathf.Clamp(fov, 50f, 90f);
        if (mainCamera != null)
            mainCamera.fieldOfView = clampedFOV;

        PlayerPrefs.SetFloat("FOV", clampedFOV);
        if (fovValueLabel != null)
            fovValueLabel.text = Mathf.RoundToInt(clampedFOV).ToString();
    }

    public void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        PlayerPrefs.SetInt("VSync", enabled ? 1 : 0);
    }

    public void SetAntiAliasing(int index)
    {
        int[] levels = { 0, 2, 4, 8 };
        if (index >= 0 && index < levels.Length)
        {
            QualitySettings.antiAliasing = levels[index];
            PlayerPrefs.SetInt("AntiAliasing", index);
        }
    }

/*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🔁 Resetting all PlayerPrefs and reloading scene...");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
*/
}
