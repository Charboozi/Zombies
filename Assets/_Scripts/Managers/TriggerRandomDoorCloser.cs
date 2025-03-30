using UnityEngine;

public class DoorTriggerRandomCloser : MonoBehaviour
{
    private float cooldownDuration = 10f;
    private bool isOnCooldown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy") || isOnCooldown) return;

        SlidingDoor door = AffectableDoorsList.Instance?.GetRandomDoor();
        if (door != null)
        {
            door.Close();
            Debug.Log($"🔒 Closed random door: {door.name}");
            StartCooldown();
        }
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        Invoke(nameof(ResetCooldown), cooldownDuration);
    }

    private void ResetCooldown()
    {
        isOnCooldown = false;
    }
}
