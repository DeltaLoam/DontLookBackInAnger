using UnityEngine;

public class StaticGhostAI : MonoBehaviour
{
    [Header("Detection & Chase Settings")]
    public float detectionRange = 25f;
    public float stopDistance = 0.1f;
    public float moveSpeed = 20f;

    [Header("Target Settings")]
    public string playerTag = "Player";
    public LayerMask detectionMask;

    [Header("Destroy Settings")]
    public float destroyDelay = 0.3f;

    private Transform target;
    private bool detected = false;

    void Update()
    {
        if (!detected)
        {
            DetectPlayer();
        }
        else
        {
            ChasePlayer();
        }
    }

    void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, detectionMask);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(playerTag))
            {
                target = hit.transform;
                detected = true;
                break;
            }
        }
    }

    void ChasePlayer()
    {
        if (target == null)
        {
            detected = false;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stopDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
