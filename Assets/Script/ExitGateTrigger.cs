using UnityEngine;

public class ExitGateTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GhostOrbManager.Instance.TryTeleport();
        }
    }
}
