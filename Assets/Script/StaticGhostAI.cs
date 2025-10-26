using UnityEngine;
using System.Collections;

[RequireComponent(typeof(JumpscareTrigger))]
public class StaticGhostAI : MonoBehaviour
{
    [Header("Detection & Chase Settings")]
    public float detectionRange = 10f;
    public float stopDistance = 2f;
    public float moveSpeed = 3.5f;

    private Transform player;
    private JumpscareTrigger trigger;
    private bool playerDetected = false;
    private bool isDestroyed = false;

    void Start()
    {
        trigger = GetComponent<JumpscareTrigger>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("StaticGhostAI: Player not found. Make sure the Player tag is assigned.");
    }

    void Update()
    {
        if (isDestroyed || player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!playerDetected && distance <= detectionRange)
        {
            playerDetected = true;
        }

        if (playerDetected && !HasTriggered(trigger))
        {
            if (distance > stopDistance)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                transform.position += direction * moveSpeed * Time.deltaTime;
                transform.LookAt(player);
            }
        }

        if (HasTriggered(trigger) && !isDestroyed)
        {
            isDestroyed = true;
            StartCoroutine(DestroyAfterScare(trigger.scareDuration));
        }
    }

    bool HasTriggered(JumpscareTrigger t)
    {
        var field = typeof(JumpscareTrigger).GetField("hasTriggered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (bool)field.GetValue(t);
    }

    IEnumerator DestroyAfterScare(float delay)
    {
        // ✅ Find AudioSource and scareSound safely
        AudioSource scareAudio = GetComponent<AudioSource>();
        AudioClip scareClip = null;

        if (trigger != null)
            scareClip = trigger.scareSound;

        // 🔊 Create independent sound source if valid clip exists
        if (scareClip != null)
        {
            GameObject tempAudio = new GameObject("TempScareAudio");
            AudioSource newAudio = tempAudio.AddComponent<AudioSource>();
            newAudio.clip = scareClip;
            newAudio.volume = (scareAudio != null) ? scareAudio.volume : 1f;
            newAudio.spatialBlend = (scareAudio != null) ? scareAudio.spatialBlend : 0f;
            newAudio.Play();
            Destroy(tempAudio, scareClip.length + 0.5f);
        }

        yield return new WaitForSeconds(delay + 0.2f);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
