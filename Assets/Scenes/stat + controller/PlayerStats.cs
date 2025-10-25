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

    // ⭐ NEW: Sanity Regeneration Settings
    [Header("Sanity Regeneration")]
    [Tooltip("Sanity จะเริ่มฟื้นฟูหลังจากหยุดลด/ใช้ไปแล้วกี่วินาที")]
    public float regenDelay = 3f;
    [Tooltip("ปริมาณ Sanity ที่จะฟื้นฟูต่อ 1 Cycle")]
    public float regenAmountPerCycle = 1f;
    [Tooltip("ระยะเวลาเป็นวินาทีระหว่างการฟื้นฟูแต่ละรอบ (1 ต่อ 2 วิ คือ 2f)")]
    public float regenCycleTime = 2f;

    // ⭐ NEW: Tracking Variables
    private float timeSinceLastSanityChange; // เวลาที่ผ่านไปตั้งแต่ Sanity มีการเปลี่ยนแปลงครั้งล่าสุด
    private float regenTimer; // ตัวจับเวลาสำหรับรอบการฟื้นฟู

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
                // ⭐ NEW: รีเซ็ตตัวจับเวลาเมื่อมีการเปลี่ยนแปลง Sanity
                timeSinceLastSanityChange = 0f;

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

        // 2. ตั้งค่าจุดเกิดเริ่มต้น (สมมติว่าคุณมี RespawnManager)
        // Note: หาก RespawnManager ไม่มีอยู่ อาจต้องลบเงื่อนไขนี้ออก
        if (RespawnManager.Instance != null)
        {
            // ใช้ 0 เป็น ID/Index สำหรับจุดเกิดเริ่มต้น หรือตำแหน่งอื่นที่เหมาะสม
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

        // ⭐ NEW: ตั้งค่า regenTimer เริ่มต้น
        regenTimer = regenCycleTime;
        timeSinceLastSanityChange = 0f;
    }

    // Checkpoint Logic
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
        //      audioSource.PlayOneShot(respawnSFX);
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

    // ⭐ NEW: Sanity Regeneration Logic
    private void RegenSanity()
    {
        // 1. ตรวจสอบเงื่อนไขการฟื้นฟู
        if (CurrentSanity >= maxSanity) return; // ถ้าเต็มแล้ว
        if (timeSinceLastSanityChange < regenDelay) return; // ยังไม่ถึงเวลาดีเลย์

        // 2. จัดการตัวจับเวลา Cycle
        regenTimer -= Time.deltaTime;
        if (regenTimer <= 0f)
        {
            // 3. ฟื้นฟู Sanity และรีเซ็ต Timer
            // การใช้ CurrentSanity += amount จะเรียก Setter และจัดการ Clamp และ Event
            CurrentSanity += regenAmountPerCycle;
            regenTimer = regenCycleTime;
        }
    }

    // Sanity Methods
    // ⭐ สำคัญ: เมธอดเหล่านี้ถูกแก้ไขให้เรียก Setter เพื่อให้รีเซ็ต Regen Delay
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

    // ⭐ เมธอด Update() ถูกนำมาใช้ในการจัดการ Sanity Regeneration
    void Update()
    {
        // ⭐ NEW: อัปเดตตัวจับเวลาสำหรับ Delay
        timeSinceLastSanityChange += Time.deltaTime;

        // เรียกใช้ฟังก์ชันฟื้นฟู Sanity
        RegenSanity();
    }
}