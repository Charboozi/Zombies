using UnityEngine;

[RequireComponent(typeof(EnemyAnimationHandler))]
public class CrawlBewaviour : MonoBehaviour
{
    [Header("Ceiling Detection")]
    public float crawlThreshold = 1.0f;  // If ceiling is closer than this → crawl

    [Header("References")]
    [SerializeField] private Transform rayOrigin; // Point near the feet of the enemy

    private float ceilingDistance = Mathf.Infinity;
    private bool isCrawling;

    private EnemyAnimationHandler enemyAnimation;

    void Awake()
    {
        enemyAnimation = GetComponent<EnemyAnimationHandler>();
    }

    void Update()
    {
        CheckCeilingDistance();
    }

    void CheckCeilingDistance()
    {
        Ray ray = new Ray(rayOrigin.position, Vector3.up);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            ceilingDistance = hit.distance;

            if (ceilingDistance <= crawlThreshold)
            {
                isCrawling = true;
            }
            else
            {
                isCrawling = false;
            }
        }
        else
        {
            // No ceiling detected
            ceilingDistance = Mathf.Infinity;
            isCrawling = false;
        }

        enemyAnimation.SetCrawling(isCrawling);
    }

    void OnDrawGizmosSelected()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = isCrawling ? Color.red : Color.green;
            Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + Vector3.up * crawlThreshold);
        }
    }
}
