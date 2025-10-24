using UnityEngine;
using System;

// Note: This class is assumed to be in the Global Namespace for easier access 
// by FirstPersonController, as per the previous successful integration.

public class PlayerStats : MonoBehaviour
{
    // --------------------------------------------------
    // I. SANITY STATS (Based on the Class Diagram)
    // --------------------------------------------------
    [Header("Sanity Stats")]
    [Tooltip("Sanity สูงสุด")]
    public float maxSanity = 100f;
    [Tooltip("Sanity ปัจจุบัน")]
    [SerializeField] private float currentSanity;

    // Events for UI/Camera Visuals
    public event Action<float, float> OnSanityUpdate; // (current, max)
    public event Action OnSanityZero;

    public float CurrentSanity
    {
        get => currentSanity;
        private set
        {
            float oldValue = currentSanity;
            currentSanity = Mathf.Clamp(value, 0f, maxSanity);

            if (oldValue != currentSanity)
            {
                OnSanityUpdate?.Invoke(currentSanity, maxSanity);
                if (currentSanity <= 0) OnSanityZero?.Invoke();
            }
        }
    }

    // --------------------------------------------------
    // II. STAMINA STATS (Based on the Class Diagram)
    // --------------------------------------------------
    [Header("Stamina Stats")]
    public float maxStamina = 100f;
    [Tooltip("อัตราการใช้ Stamina ต่อวินาทีเมื่อวิ่ง")]
    [SerializeField] public float staminaDrainRate = 15f;
    [Tooltip("อัตราการฟื้นฟู Stamina ต่อวินาที")]
    [SerializeField] public float regenRate = 5f;

    [SerializeField] private float currentStamina;
    [SerializeField] private bool isExhausted = false;

    public float CurrentStamina => currentStamina;
    public bool IsExhausted => isExhausted;

    // Event for Stamina Bar UI
    public event Action<float, float> OnStaminaUpdate;
    public event Action OnExhausted;
    public event Action OnNotExhausted;

    // --------------------------------------------------

    void Awake()
    {
        Initialize(maxSanity, maxStamina);
    }

    public void Initialize(float s, float m)
    {
        CurrentSanity = s;
        currentStamina = m;
        isExhausted = false;
        OnSanityUpdate?.Invoke(currentSanity, maxSanity);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina);
    }

    // --------------------------------------------------
    // SANITY METHODS
    // --------------------------------------------------
    public void LoseSanity(float amount)
    {
        CurrentSanity -= amount;
    }

    public void RecoverSanity(float amount)
    {
        CurrentSanity += amount;
    }

    // --------------------------------------------------
    // STAMINA METHODS (Called by FirstPersonController)
    // --------------------------------------------------

    public void UseStamina(float amount)
    {
        if (isExhausted) return;

        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina); // Notify UI

        if (currentStamina <= 0) CheckExhaustion(true);
    }

    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(maxStamina, currentStamina);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina); // Notify UI

        // Recover from exhaustion when stamina is above 20%
        if (currentStamina > maxStamina * 0.2f && isExhausted)
        {
            CheckExhaustion(false);
        }
    }

    private void CheckExhaustion(bool exhausted)
    {
        if (isExhausted == exhausted) return;

        isExhausted = exhausted;
        if (isExhausted) OnExhausted?.Invoke();
        else OnNotExhausted?.Invoke();
    }
}