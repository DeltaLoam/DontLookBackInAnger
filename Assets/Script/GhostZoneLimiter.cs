using UnityEngine;

public class GhostZoneLimiter : MonoBehaviour
{
    [Header("Ghost Zone Settings")]
    [SerializeField] private Vector3 returnPosition = Vector3.zero; // editable in Inspector
    [SerializeField] private bool resetRotation = true; // optional toggle

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Ghost")) return;

        // Teleport the ghost back inside Zone 2
        other.transform.position = returnPosition;

        if (resetRotation)
            other.transform.rotation = Quaternion.identity;

        Debug.Log($"{other.name} tried to leave its zone. Sent back to {returnPosition}");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(returnPosition, 0.5f);
        Gizmos.DrawLine(transform.position, returnPosition);
    }
#endif
}
