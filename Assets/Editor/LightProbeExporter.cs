using UnityEditor;
using UnityEngine;

public class LightProbeExporter : EditorWindow
{
    private string assetName = "LightProbes_Exported";

    [MenuItem("Tools/Export Light Probes")]
    public static void ShowWindow()
    {
        GetWindow<LightProbeExporter>("Export Light Probes");
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Current Scene Light Probes", EditorStyles.boldLabel);
        assetName = EditorGUILayout.TextField("Asset Name", assetName);

        if (GUILayout.Button("Export"))
        {
            ExportLightProbes();
        }
    }

    private void ExportLightProbes()
    {
        var currentProbes = LightmapSettings.lightProbes;

        if (currentProbes == null || currentProbes.count == 0)
        {
            Debug.LogError("No baked light probes found in the current scene!");
            return;
        }

        // Clone the existing light probes (you cannot create LightProbes with 'new')
        var clonedProbes = Object.Instantiate(currentProbes);

        // Save the cloned light probes as an asset
        string path = $"Assets/{assetName}.asset";
        AssetDatabase.CreateAsset(clonedProbes, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Light Probes exported successfully to {path}");
    }
}
