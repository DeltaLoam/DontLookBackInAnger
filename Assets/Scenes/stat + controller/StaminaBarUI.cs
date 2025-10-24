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

    public void SetTarget(PlayerStats stats)
    {
        if (targetStats != null)
        {
            targetStats.OnStaminaUpdate -= UpdateStaminaBar;
        }

        targetStats = stats;

        if (targetStats != null)
        {
            targetStats.OnStaminaUpdate += UpdateStaminaBar;
            UpdateStaminaBar(targetStats.CurrentStamina, targetStats.maxStamina);
        }
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBarImage == null) return;

        float fillAmount = currentStamina / maxStamina;
        staminaBarImage.fillAmount = fillAmount;
    }

    void OnDestroy()
    {
        if (targetStats != null)
        {
            targetStats.OnStaminaUpdate -= UpdateStaminaBar;
        }
    }
}