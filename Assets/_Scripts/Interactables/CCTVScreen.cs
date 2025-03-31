using UnityEngine;

public class CCTVScreen : MonoBehaviour, IClientOnlyAction
{
    public void DoClientAction()
    {
        var manager = FindFirstObjectByType<CCTVManager>();
        if (manager != null)
        {
            manager.ActivateCCTV();
        }
    }
}