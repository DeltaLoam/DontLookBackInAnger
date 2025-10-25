using UnityEngine;

public class EndingSceneTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Canvas endingCanvas; // Drag your canvas here

    [Header("Audio Settings")]
    [SerializeField] private AudioClip endingSound; // Drag your sound file here
    [SerializeField] private float volume = 1f;

    private bool triggered = false;

    private void Start()
    {
        if (endingCanvas != null)
            endingCanvas.enabled = false; // Hide canvas at start
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            PlayEnding();
        }
    }

    private void PlayEnding()
    {
        // Show the canvas
        if (endingCanvas != null)
            endingCanvas.enabled = true;

        // Play the audio
        if (endingSound != null)
            AudioSource.PlayClipAtPoint(endingSound, transform.position, volume);
    }
}
