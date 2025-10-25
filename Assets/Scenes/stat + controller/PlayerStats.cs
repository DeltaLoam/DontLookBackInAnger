using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    // --------------------------------------------------
    // I. SANITY STATS 
    // --------------------------------------------------
    [Header("Sanity Stats")]
    public float maxSanity = 100f;
    [SerializeField] private float currentSanity;

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
                // เรียก SanityVisual ให้ Update Visual Effects
                OnSanityUpdate?.Invoke(currentSanity, maxSanity);
                if (currentSanity <= 0) OnSanityZero?.Invoke();
            }
        }
    }

    // --------------------------------------------------
    // II. STAMINA STATS
    // --------------------------------------------------
    [Header("Stamina Stats")]
    public float maxStamina = 100f;
    [SerializeField] public float staminaDrainRate = 15f;
    [SerializeField] public float regenRate = 5f;

    [SerializeField] private float currentStamina;
    [SerializeField] private bool isExhausted = false;

    public float CurrentStamina => currentStamina;
    public bool IsExhausted => isExhausted;

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

    // ⭐ DEBUG INPUT FOR SANITY (แก้ไขให้ K ลดต่อเนื่อง)
    void Update()
    {
        // 🔴 K: ลด Sanity อย่างต่อเนื่อง (ใช้ GetKey และ Time.deltaTime)
        if (Input.GetKey(KeyCode.K))
        {
            // ลด 5 หน่วยต่อวินาที (สามารถปรับค่า 5f ได้ตามต้องการ)
            LoseSanity(5f * Time.deltaTime);
            // Debug.Log($"Sanity Draining. Current: {CurrentSanity}"); // เปิดถ้าต้องการดูค่าทุกเฟรม
        }

        // 🟢 L: เพิ่ม Sanity 10 หน่วย ต่อการกด 1 ครั้ง (ใช้ GetKeyDown)
        if (Input.GetKeyDown(KeyCode.L))
        {
            RecoverSanity(10f);
            Debug.Log($"Sanity Recovered to: {CurrentSanity}");
        }
    }

    // Sanity Methods
    public void LoseSanity(float amount) { CurrentSanity -= amount; }
    public void RecoverSanity(float amount) { CurrentSanity += amount; }

    // Stamina Methods (เหมือนเดิม)
    public void UseStamina(float amount)
    {
        if (isExhausted) return;
        currentStamina -= amount;
        currentStamina = Mathf.Max(0, currentStamina);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina);
        if (currentStamina <= 0) CheckExhaustion(true);
    }

    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Min(maxStamina, currentStamina);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina);

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