using UnityEngine;

public class CCTVScreen : MonoBehaviour, IClientOnlyAction, IInteractableAction
{
    public void DoClientAction()
    {
        var manager = FindFirstObjectByType<CCTVManager>();
        if (manager != null)
        {
            manager.ActivateCCTV();
        }
    }
    public void DoAction(){}
}