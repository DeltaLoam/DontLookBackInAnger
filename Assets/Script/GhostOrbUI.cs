using TMPro;
using UnityEngine;

public class GhostOrbUI : MonoBehaviour
{
    public TMP_Text orbText; // Assign this in Inspector

    public void UpdateUI()
    {
        if (GhostOrbManager.Instance != null && orbText != null)
        {
            orbText.text = $"Ghost Orbs: {GhostOrbManager.Instance.totalGhostOrbs} / {GhostOrbManager.Instance.requiredOrbsToExit}";
        }
    }
}
