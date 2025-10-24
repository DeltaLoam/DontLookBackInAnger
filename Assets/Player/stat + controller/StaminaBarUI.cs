using UnityEngine;
using UnityEngine.UI;
using System;

public class StaminaBarUI : MonoBehaviour
{
    private PlayerStats targetStats;
    private Image staminaBarImage;

    void Awake()
    {
        staminaBarImage = GetComponent<Image>();
        if (staminaBarImage == null)
        {
            Debug.LogError("StaminaBarUI requires an Image component on this GameObject.");
            enabled = false;
        }
    }

    /// <summary>
    /// Establishes the connection to a specific PlayerStats component.
    /// This should be called by a Manager or Player initialization script.
    /// </summary>
    public void SetTarget(PlayerStats stats)
    {
        // Unsubscribe from old target
        if (targetStats != null)
        {
            targetStats.OnStaminaUpdate -= UpdateStaminaBar;
        }

        targetStats = stats;

        // Subscribe to new target
        if (targetStats != null)
        {
            targetStats.OnStaminaUpdate += UpdateStaminaBar;

            // Initial update
            UpdateStaminaBar(targetStats.CurrentStamina, targetStats.maxStamina);
        }
    }

    /// <summary>
    /// Updates the UI bar based on the received stamina values (called by event).
    /// </summary>
    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBarImage == null) return;

        // Calculate the fill amount (0 to 1)
        float fillAmount = currentStamina / maxStamina;
        staminaBarImage.fillAmount = fillAmount;
    }

    void OnDestroy()
    {
        // Crucial: Unsubscribe when the UI object is destroyed
        if (targetStats != null)
        {
            targetStats.OnStaminaUpdate -= UpdateStaminaBar;
        }
    }
}