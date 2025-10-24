using UnityEngine;

public class UIAssigner : MonoBehaviour
{
    [Header("UI References")]
    public StaminaBarUI staminaBarUI;

    private PlayerStats playerStats;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        if (playerStats != null && staminaBarUI != null)
        {
            staminaBarUI.SetTarget(playerStats);
            Debug.Log("UI successfully connected to PlayerStats.");
        }
        else
        {
            Debug.LogError("UIAssigner failed to connect UI/Stats. Check references.");
        }
    }
}