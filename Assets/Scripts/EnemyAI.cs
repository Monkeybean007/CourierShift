using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    private Transform player;

    [Header("Aggression")]
    public float baseDetectionRange = 8f;
    public float maxDetectionRange = 20f;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    private float attackTimer = 0f;

    private void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        // Scale aggression based on packages collected
        int collected = GameManager.Instance != null ? GameManager.Instance.GetCollectedPackages() : 0;
        int total = GameManager.Instance != null ? GameManager.Instance.totalPackagesNeeded : 1;

        float progress = Mathf.Clamp01((float)collected / total);
        float detectionRange = Mathf.Lerp(baseDetectionRange, maxDetectionRange, progress);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            agent.SetDestination(player.position);

            if (distance <= attackRange && attackTimer <= 0f)
            {
                AttackPlayer();
            }
        }

        if (attackTimer > 0) attackTimer -= Time.deltaTime;
    }

    void AttackPlayer()
    {
        Debug.Log($"{name} attacked the player!");
        attackTimer = attackCooldown;
    }

    public void HearNoise(Vector3 noisePos)
    {
        if (agent != null)
        {
            agent.SetDestination(noisePos);
        }
    }
}
