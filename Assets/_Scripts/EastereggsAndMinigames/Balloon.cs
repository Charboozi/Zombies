using UnityEngine;

public class Balloon : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        TryPop();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPop();
    }

    public void TryPop()
    {
        // Prevent double pops
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false); // hide the balloon

        BalloonMinigameManager.Instance?.OnBalloonPopped();

        Destroy(gameObject); // destroy after short delay if needed
    }
}
