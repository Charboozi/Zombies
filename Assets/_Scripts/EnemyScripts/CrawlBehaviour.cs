using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(EnemyAnimationHandler))]
public class CrawlBehaviour : NetworkBehaviour
{
    [Header("Ceiling Detection")]
    public float crawlThreshold = 1.0f;
    public float crawlBuffer = 0.1f;

    [Header("Ray Origin")]
    [SerializeField] private Transform rayOrigin;

    [Tooltip("Layer(s) considered as ceiling.")]
    public LayerMask ceilingLayer;

    private float ceilingDistance = Mathf.Infinity;
    private bool isCrawling;

    private EnemyAnimationHandler enemyAnimation;

    void Awake()
    {
        enemyAnimation = GetComponent<EnemyAnimationHandler>();
    }

    void Update()
    {
        if (!IsServer) return; // Only server runs logic

        CheckCeilingDistance();
    }

    void CheckCeilingDistance()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, Vector3.up);

        bool shouldCrawl;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ceilingLayer))
        {
            ceilingDistance = hit.distance;

            if (!isCrawling)
                shouldCrawl = ceilingDistance <= crawlThreshold - crawlBuffer;
            else
                shouldCrawl = ceilingDistance <= crawlThreshold + crawlBuffer;
        }
        else
        {
            ceilingDistance = Mathf.Infinity;
            shouldCrawl = false;
        }

        if (shouldCrawl != isCrawling)
        {
            isCrawling = shouldCrawl;
            UpdateCrawlClientRpc(isCrawling);
        }
    }

    [ClientRpc]
    void UpdateCrawlClientRpc(bool crawl)
    {
        isCrawling = crawl;
        enemyAnimation.SetCrawling(crawl);
    }
}
