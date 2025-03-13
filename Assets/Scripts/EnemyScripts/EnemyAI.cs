using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using System.Collections;

public class EnemyAI : NetworkBehaviour
{
    [Header("AI Settings")]
    public float roamRadius = 10f;
    public float roamDelay = 3f;
    public float chaseRange = 10f;
    public float attackRange = 2f; // Attack distance
    public float attackRate = 1.5f; // Time between attacks
    public int attackDamage = 10; // Damage per attack
    public LayerMask playerLayer;

    [Header("Animation")]
    public Animator animator;

    [Header("Powerup Settings")]
    [Range(0f, 1f)] public float powerupSpawnChance = 0.3f;

    private NavMeshAgent agent;
    private Transform targetPlayer;
    private float roamTimer;
    private bool isAttacking = false; // Prevent multiple attack coroutines
    private bool isDead = false;
    private GameObject spawnedPowerup; // Store reference to spawned powerup

    private float checkPlayerInterval = 2f; // How often to check for a closer player
    private float checkPlayerTimer = 0f; // Timer for checking players

    public event System.Action OnEnemyDeath;

    public override void OnNetworkSpawn()
    {
        if (IsServer) // Ensure only the server runs AI logic
        {
            if (agent == null) // Prevent null reference
            {
                agent = GetComponent<NavMeshAgent>();
            }

            if (agent != null) // Double-check before using it
            {
                PickNewRoamDestination();
            }
        }
        AssignRandomWalkAnimation();
    }

    private void AssignRandomWalkAnimation()
    {
        if (animator != null)
        {
            int randomIndex = Random.Range(0, 3); // Choose between 3 different walk animations
            animator.SetInteger("walkIndex", randomIndex);
        }
    }


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!IsServer || isDead) return;

        checkPlayerTimer -= Time.deltaTime;
        if (checkPlayerTimer <= 0f) // Every few seconds, check for a new closest player
        {
            LookForPlayer();
            checkPlayerTimer = checkPlayerInterval; // Reset timer
        }

        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= attackRange)
            {
                StartAttacking();
            }
            else
            {
                isAttacking = false; // Stop attacking if player moves away
                animator.SetBool("isAttacking", false);
                agent.isStopped = false;
                ChasePlayer();
            }
        }
        else
        {
            Roam();
        }
    }

    void LookForPlayer()
    {
        Collider[] players = Physics.OverlapSphere(transform.position, chaseRange, playerLayer);

        if (players.Length > 0)
        {
            Transform closestPlayer = null;
            float shortestDistance = Mathf.Infinity;

            foreach (Collider player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestPlayer = player.transform;
                }
            }

            // If the closest player is not the current target, switch targets
            if (closestPlayer != null && closestPlayer != targetPlayer)
            {
                targetPlayer = closestPlayer;
            }
        }
    }


    void ChasePlayer()
    {
        if (targetPlayer == null) return;
        agent.SetDestination(targetPlayer.position);
    }

    void Roam()
    {
        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f || agent.remainingDistance < 1f)
        {
            PickNewRoamDestination();
        }
    }

    void PickNewRoamDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            roamTimer = roamDelay;
        }
    }

    void StartAttacking()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            animator.SetBool("isAttacking", true);
            agent.isStopped = true; // Stop moving when attacking
            StartCoroutine(AttackPlayer());
        }
    }

    IEnumerator AttackPlayer()
    {
        while (targetPlayer != null && !isDead && Vector3.Distance(transform.position, targetPlayer.position) <= attackRange)
        {
            Debug.Log("Enemy attacking player!");

            // Play attack animation
            animator.SetBool("isAttacking", true);

            // Wait for animation to reach damage frame (adjust timing as needed)
            yield return new WaitForSeconds(0.6f); 

            if(targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= attackRange)
            {
                // Damage the player using ServerRpc
                EntityHealth playerHealth = targetPlayer.GetComponent<EntityHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamageServerRpc(attackDamage);
                }
            }

            // Wait before attacking again
            yield return new WaitForSeconds(attackRate - 0.5f);
        }

        // Reset attack state when the player moves away
        isAttacking = false;
        agent.isStopped = false;
        animator.SetBool("isAttacking", false);
    }

    public void PlayDeathAnimation()
    {
        if (animator != null)
        {
            isDead = true;
            animator.SetBool("isDead", true);
            isAttacking = false;
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
        }

        StopAllCoroutines(); 
        agent.isStopped = true;
        animator.SetBool("isAttacking", false);

        OnEnemyDeath?.Invoke();

        if (IsServer)
        {
            SpawnPowerupServerRpc();
        }

        StartCoroutine(DestroyAfterDelay(10f));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (IsServer)
        {
            // Explicitly despawn and destroy the powerup before the enemy despawns
            if (spawnedPowerup != null)
            {
                NetworkObject powerupNetworkObject = spawnedPowerup.GetComponent<NetworkObject>();
                if (powerupNetworkObject != null && powerupNetworkObject.IsSpawned)
                {
                    powerupNetworkObject.Despawn(true); // Despawn on network
                }
                Destroy(spawnedPowerup); // Destroy locally
            }

            // Now safely despawn and destroy the enemy
            NetworkObject enemyNetworkObject = GetComponent<NetworkObject>();
            if (enemyNetworkObject != null && enemyNetworkObject.IsSpawned)
            {
                enemyNetworkObject.Despawn(true);
            }
            Destroy(gameObject);
        }
    }

    [ServerRpc]
    private void SpawnPowerupServerRpc()
    {
        if (Random.value > powerupSpawnChance) // 70% chance to NOT spawn a powerup
        {
            Debug.Log("No powerup spawned.");
            return;
        }

        GameObject[] powerups = Resources.LoadAll<GameObject>("Powerups");
        if (powerups.Length == 0) return;

        GameObject randomPowerup = powerups[Random.Range(0, powerups.Length)];
        spawnedPowerup = Instantiate(randomPowerup, transform.position, Quaternion.Euler(-90f, 0f, 0f), transform); // Make powerup a child of enemy
        // ✅ Move powerup to local position (0,0,0) relative to the enemy
        spawnedPowerup.transform.localPosition = new Vector3(0, -1f);

        NetworkObject networkObject = spawnedPowerup.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(true);
        }
        else
        {
            Debug.LogError("Powerup prefab is missing NetworkObject component.");
        }
    }
}
