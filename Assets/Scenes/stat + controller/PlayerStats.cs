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

    public event Action<float, float> OnSanityUpdate; 
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
                if (currentSanity <= 0 && oldValue > 0)
                {
                    OnSanityZero?.Invoke();
                    HandleDeath(); 
                }
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
    // III. LIFE STATS
    // --------------------------------------------------
    [Header("Life Stats")]
    public int maxLives = 3; 
    [SerializeField] private int currentLife; 

    public int CurrentLife => currentLife;

    public event Action<int> OnLifeUpdate; 
    public event Action OnGameOver; 

    // --------------------------------------------------
    // IV. RESPAWN LOGIC & AUDIO
    // --------------------------------------------------
    [Header("Respawn & Audio")]
    [Tooltip("ตำแหน่งเกิดใหม่ที่ถูกบันทึกไว้ล่าสุด")]
    private Vector3 respawnPosition; 

    [Tooltip("AudioSource สำหรับเล่นเสียง")]
    private AudioSource audioSource; 

    // [Tooltip("เสียงที่จะเล่นเมื่อผู้เล่นเกิดใหม่")]
    // public AudioClip respawnSFX; 
    
    // --------------------------------------------------
    
    void Awake()
    {
        // 1. เตรียม AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 2. ตั้งค่าจุดเกิดเริ่มต้น
        if (RespawnManager.Instance != null)
        {
            respawnPosition = RespawnManager.Instance.GetRespawnPosition(0); 
        }
        else
        {
            respawnPosition = transform.position; 
            Debug.LogWarning("RespawnManager not found. Using current position as initial spawn point.");
        }

        Initialize(maxSanity, maxStamina);
    }

    public void Initialize(float s, float m)
    {
        CurrentSanity = s;
        currentStamina = m;
        isExhausted = false;
        currentLife = maxLives;
        OnSanityUpdate?.Invoke(currentSanity, maxSanity);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina);
        OnLifeUpdate?.Invoke(currentLife);
    }
    
    // Checkpoint Logic
    /// <summary>
    /// กำหนดตำแหน่งปัจจุบันของผู้เล่นให้เป็นจุดเกิดใหม่ (Checkpoint)
    /// </summary>
    public void SetRespawnPoint()
    {
        respawnPosition = transform.position;
        Debug.Log($"Respawn Point set to: {respawnPosition}");
    }

    // Respawn Logic
    public void RespawnPlayer()
    {
        // 1. รีเซ็ตค่าสถิติ
        CurrentSanity = maxSanity;
        currentStamina = maxStamina;
        isExhausted = false;

        // 2. Teleport
        transform.position = respawnPosition;
        
        // 3. เล่นเสียงเอฟเฟกต์ Respawn
        // if (respawnSFX != null && audioSource != null)
        // {
        //     audioSource.PlayOneShot(respawnSFX);
        // }
        
        Debug.Log($"Player Respawned at: {respawnPosition}");
    }

    private void HandleDeath()
    {
        if (currentLife <= 0)
        {
            OnGameOver?.Invoke();
            Debug.Log("GAME OVER! No lives remaining.");
            return;
        }

        currentLife--;
        OnLifeUpdate?.Invoke(currentLife);

        RespawnPlayer();
    }
    
    // Sanity Methods
    public void ApplySanityDrain(float amount) { CurrentSanity -= amount; }
    public void LoseSanity(float amount) { CurrentSanity -= amount; }
    public void RecoverSanity(float amount) { CurrentSanity += amount; }

    // Stamina Methods
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

    // ⭐ DEBUG INPUT REMOVED: เมธอด Update() ไม่มีโค้ดปุ่มกดแล้ว
    void Update()
    {
        // โค้ด Update ว่างเปล่า
    }
}