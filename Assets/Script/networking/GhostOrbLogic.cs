using UnityEngine;

// This is a plain MonoBehaviour. It has NO FUSION CODE.
// It holds the data and performs the core actions.
public class GhostOrbLogic : MonoBehaviour
{
    // A singleton so your UI scripts can easily find and reference this logic.
    public static GhostOrbLogic Instance { get; private set; }

    [Header("Data and Settings")]
    public int totalGhostOrbs = 0;
    public int requiredOrbsToExit = 4;
    public string nextSceneName;

    [Header("Effects and UI")]
    public GhostOrbUI orbUI;
    public AudioClip collectSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate managers
        }
    }

    /// <summary>
    /// Force-sets the orb count. The network script will call this.
    /// </summary>
    public void SetOrbCount(int count)
    {
        totalGhostOrbs = count;
    }

    /// <summary>
    /// Updates the UI display with the current orb count.
    /// </summary>
    public void UpdateDisplay()
    {
        if (orbUI != null)
        {
            orbUI.UpdateUI(); // Assumes your UI script reads the public 'totalGhostOrbs' variable
        }
    }

    /// <summary>
    /// Plays the collection sound effect locally.
    /// </summary>
    public void PlayCollectSound()
    {
        if (collectSound != null && Camera.main != null)
        {
            // Play a 2D sound that is not attached to any object
            AudioSource.PlayClipAtPoint(collectSound, Camera.main.transform.position);
        }
    }
}
