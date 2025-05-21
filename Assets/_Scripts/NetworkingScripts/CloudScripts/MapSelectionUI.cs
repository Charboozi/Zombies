using UnityEngine;

public class MapSelectionUI : MonoBehaviour
{
    public void OnMapSelected(string mapName)
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SetMapName(mapName);
        }
    }
}
