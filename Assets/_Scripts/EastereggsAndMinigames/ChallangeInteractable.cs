using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(Interactable))]
public class ChallengeInteractable : NetworkBehaviour, IInteractableAction
{
    [Header("Challenge Settings")]
    [SerializeField] private float challengeTimeLimit = 200f;

    [Header("Reward Settings")]
    [SerializeField] private TeleporterInteractable rewardTeleporter;
    [SerializeField] private float rewardDuration = 20f;

    private bool challengeStarted = false;
    private bool challengeCompleted = false;

    public void DoAction()
    {
        Debug.Log($"[Challenge] DoAction() called on {(IsServer? "Server" : "Client")}, started={challengeStarted}, completed={challengeCompleted}");
        if (!IsServer || challengeStarted || challengeCompleted) return;
        if (rewardTeleporter == null)
        {
            Debug.LogError("⚠️ ChallengeInteractable: rewardTeleporter is NULL!");
            return;
        }
        Debug.Log("[Challenge] Starting coroutine");
        StartCoroutine(ChallengeRoutine());
    }
    private IEnumerator ChallengeRoutine()
    {
        challengeStarted = true;
        bool enemyKilled = false;

        void OnSpecialKilled(EntityHealth e) => enemyKilled = true;
        EntityHealth.OnSpecialEnemyKilled += OnSpecialKilled;

        float timer = 0f;
        while (timer < challengeTimeLimit && !enemyKilled)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        EntityHealth.OnSpecialEnemyKilled -= OnSpecialKilled;

        if (enemyKilled)
        {
            Debug.Log("[Challenge] Success! Enabling teleporter override.");
            rewardTeleporter.OverrideDestinationTemporarily(rewardDuration);
            challengeCompleted = true;
        }
        else
        {
            Debug.Log("[Challenge] Failed—time's up.");
        }

        challengeStarted = false;
    }
}
