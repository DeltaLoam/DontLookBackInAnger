using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostOrbManager : MonoBehaviour
{
    public static GhostOrbManager Instance;

    [Header("Orb Settings")]
    public int totalGhostOrbs = 0;
    public int requiredOrbsToExit = 4;

    [Header("Scene Settings")]
    public string nextSceneName;

    [Header("UI")]
    public GhostOrbUI orbUI; // Reference to the UI script

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddOrb()
    {
        totalGhostOrbs++;
        Debug.Log($"[GhostOrbManager] Ghost Orbs Collected: {totalGhostOrbs}");

        // Update UI
        if (orbUI != null)
            orbUI.UpdateUI();
    }

    public void TryTeleport()
    {
        if (totalGhostOrbs >= requiredOrbsToExit)
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log("[GhostOrbManager] Enough orbs collected! Teleporting...");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[GhostOrbManager] Next scene name not set!");
            }
        }
        else
        {
            Debug.Log($"[GhostOrbManager] Need {requiredOrbsToExit - totalGhostOrbs} more orbs.");
        }
    }
}
