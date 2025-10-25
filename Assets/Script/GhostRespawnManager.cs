using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GhostRespawnManager : MonoBehaviour
{
    [Header("Ghost Settings")]
    public GameObject ghostPrefab;
    public Transform[] spawnPoints;
    public float respawnDelay = 10f;

    private GameObject currentGhost;

    private void Start()
    {
        SpawnGhost();
    }

    private void SpawnGhost()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Debug.Log("Spawning ghost at: " + spawnPoint.position);

        currentGhost = Instantiate(
            ghostPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        TeleportChildGhost(currentGhost, spawnPoint.position);

        // Monitor jumpscare via the scareImage
        JumpscareTrigger trigger = currentGhost.GetComponentInChildren<JumpscareTrigger>();
        if (trigger != null && trigger.scareImage != null)
        {
            StartCoroutine(MonitorJumpscare(trigger.scareImage, trigger));
        }
    }

    private void TeleportChildGhost(GameObject ghostRoot, Vector3 position)
    {
        Transform child = ghostRoot.transform.Find("Ghost");
        if (child != null)
        {
            child.position = position;
            child.rotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning("Child named 'Ghost' not found!");
        }
    }

    private IEnumerator MonitorJumpscare(Image scareImage, JumpscareTrigger trigger)
    {
        // Wait until jumpscare image becomes visible
        while (!scareImage.enabled)
        {
            yield return null;
        }

        Debug.Log("Ghost triggered jumpscare. Waiting for scare to finish.");

        // Wait for the jumpscare duration
        yield return new WaitForSeconds(trigger.scareDuration);

        // Detach and play the sound independently
        if (trigger.scareSound != null && trigger.GetComponent<AudioSource>() != null)
        {
            AudioSource audioSource = trigger.GetComponent<AudioSource>();
            audioSource.transform.parent = null; // detach from ghost
            audioSource.Play();
            Destroy(audioSource.gameObject, trigger.scareSound.length + 0.1f); // cleanup after finished
        }

        // Destroy ghost after scare finishes
        if (currentGhost != null)
            Destroy(currentGhost);

        Debug.Log("Ghost destroyed. Respawning in " + respawnDelay + " seconds.");

        // Wait for respawn delay and spawn ghost again
        yield return new WaitForSeconds(respawnDelay);
        SpawnGhost();
    }
}
