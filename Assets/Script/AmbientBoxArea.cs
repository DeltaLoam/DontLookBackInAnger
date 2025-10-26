using UnityEngine;
using Fusion;

[RequireComponent(typeof(AudioSource))]
public class AmbientBoxArea : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private BoxCollider area;
    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("Audio Settings")]
    [SerializeField] private bool playOnStart = false;

    private AudioSource audioSource;
    private Transform localPlayer;
    private float targetVolume;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void Start()
    {
        if (playOnStart)
        {
            audioSource.volume = 1f;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (localPlayer == null || area == null)
            return;

        bool inside = area.bounds.Contains(localPlayer.position);
        targetVolume = inside ? 1f : 0f;

        // Smooth fade in/out
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        if (audioSource.volume > 0f && !audioSource.isPlaying)
            audioSource.Play();
        else if (audioSource.volume <= 0f && audioSource.isPlaying)
            audioSource.Stop();
    }

    public void AssignLocalPlayer(Transform playerTransform)
    {
        localPlayer = playerTransform;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (area == null) return;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.25f);
        Gizmos.DrawCube(area.bounds.center, area.bounds.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(area.bounds.center, area.bounds.size);
    }
#endif
}