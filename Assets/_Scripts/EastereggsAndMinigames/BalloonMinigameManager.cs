using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class BalloonMinigameManager : MonoBehaviour
{
    public static BalloonMinigameManager Instance;

    [Header("Settings")]
    public float gameDuration = 100f;
    public int numberOfBalloons = 3;
    public GameObject balloonPrefab;

    [Header("Spawn Points")]
    public List<BalloonSpawnPoint> spawnPoints;

    private List<GameObject> spawnedBalloons = new();
    private float timer;
    private bool isActive = false;

    private int balloonsPopped = 0;
    private int balloonsSpawned = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EndMinigame(success: false); // time ran out
        }
    }

    public void StartMinigame()
    {
        if (isActive) return;

        isActive = true;
        timer = gameDuration;
        balloonsPopped = 0;

        SpawnBalloons();
        Debug.Log("🎈 Balloon minigame started!");
    }

    private void SpawnBalloons()
    {
        spawnedBalloons.Clear();

        var shuffled = new List<BalloonSpawnPoint>(spawnPoints);
        Shuffle(shuffled);

        int spawnCount = Mathf.Min(numberOfBalloons, shuffled.Count);
        balloonsSpawned = spawnCount;

        for (int i = 0; i < spawnCount; i++)
        {
            var pos = shuffled[i].transform.position;
            var balloon = Instantiate(balloonPrefab, pos, Quaternion.identity);
            spawnedBalloons.Add(balloon);
        }
    }

    public void OnBalloonPopped()
    {
        balloonsPopped++;

        if (balloonsPopped >= balloonsSpawned && isActive)
        {
            EndMinigame(success: true);
        }
    }

    private void EndMinigame(bool success)
    {
        if (!isActive) return;

        isActive = false;

        foreach (var balloon in spawnedBalloons)
        {
            if (balloon != null)
                Destroy(balloon);
        }

        spawnedBalloons.Clear();

        if (success)
        {
            Debug.Log("✅ All balloons popped in time! Reward granted.");
            GiveReward();
        }
        else
        {
            Debug.Log("❌ Minigame failed — time ran out.");
        }
    }

    [SerializeField] private BalloonRewardSpawner rewardSpawner;

    private void GiveReward()
    {
        if (rewardSpawner != null)
        {
            StartCoroutine(DelayedRewardSpawn());
        }
    }

    private IEnumerator DelayedRewardSpawn()
    {
        yield return new WaitForSeconds(10f);
        rewardSpawner.SpawnRandomReward();
    }

    private void Shuffle(List<BalloonSpawnPoint> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
