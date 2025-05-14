using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown  resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown  qualityDropdown;

    private Resolution[] resolutions;

    private void Start()
    {
        SetupResolutions();
        SetupQualityDropdown();
        LoadSettings();

        // Hook up listeners
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
    }

    private void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string resOption = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(resOption);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // Use saved index if available
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupQualityDropdown()
    {
        qualityDropdown.ClearOptions();

        List<string> qualities = new List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(qualities);

        int savedQuality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        qualityDropdown.value = savedQuality;
        qualityDropdown.RefreshShownValue();
    }

    private void LoadSettings()
    {
        int resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", resolutionDropdown.value);
        Resolution res = resolutions[resolutionIndex];
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        int quality = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());

        Screen.SetResolution(res.width, res.height, isFullscreen);
        Screen.fullScreen = isFullscreen;
        QualitySettings.SetQualityLevel(quality);

        resolutionDropdown.value = resolutionIndex;
        fullscreenToggle.isOn = isFullscreen;
        qualityDropdown.value = quality;
    }

    public void SetResolution(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    public void SetQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex);
    }
}
