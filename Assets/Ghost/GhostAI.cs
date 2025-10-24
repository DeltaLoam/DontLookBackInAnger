using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("References")]
    public Animator animator;                // Drag the Model (child with Animator) here
    public Transform player;                 // Drag the player (EasyPeasyFirstPersonController) here

    [Header("AI Settings")]
    public float detectionRange = 10f;
    public float walkRadius = 5f;
    public float walkSpeed = 1.5f;
    public float runSpeed = 4f;

    private NavMeshAgent agent;
    private Vector3 walkTarget;
    private bool isChasing = false;
    private float idleTimer = 0f;
    private float idleDuration = 3f; // how long ghost stays idle before wandering

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Auto-find animator if not assigned manually
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // Chase player
            isChasing = true;
            agent.speed = runSpeed;
            agent.SetDestination(player.position);
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        }
        else
        {
            // Idle or random walk
            isChasing = false;
            animator.SetBool("isRunning", false);

            idleTimer += Time.deltaTime;

            if (idleTimer >= idleDuration)
            {
                // Pick a new random point to walk to
                Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
                randomDirection += transform.position;

                if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, 1))
                {
                    walkTarget = hit.position;
                    agent.speed = walkSpeed;
                    agent.SetDestination(walkTarget);
                    animator.SetBool("isWalking", true);
                }

                idleTimer = 0f;
                idleDuration = Random.Range(3f, 6f); // random pause time before next walk
            }

            // If ghost reached destination, idle again
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                animator.SetBool("isWalking", false);
            }
        }
    }
}
