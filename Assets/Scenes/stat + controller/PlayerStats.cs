using UnityEngine;
using System;
using Fusion;

public class PlayerStats : NetworkBehaviour
{
    // ⭐ STATIC ACTION: ส่งสัญญาณ Local Player ให้ UI เชื่อมต่อ
    public static Action<PlayerStats> OnLocalPlayerStatsReady;

    // --------------------------------------------------
    // I. SANITY STATS 
    // --------------------------------------------------
    [Header("Sanity Stats")]
    public float maxSanity = 100f;
    [Networked]
    [SerializeField]
    private float currentSanity { get; set; }

    [Header("Sanity Regeneration")]
    public float regenDelay = 3f;
    public float regenAmountPerCycle = 1f;
    public float regenCycleTime = 2f;

    private float timeSinceLastSanityChange;
    private float regenTimer;

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

                if (Object.HasInputAuthority)
                {
                    timeSinceLastSanityChange = 0f;
                }

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

    // ⭐⭐⭐ แก้ไข CS1061: ให้ StaminaSync/StaminaBarUI อ่านค่าได้ ⭐⭐⭐
    public float CurrentStaminaReadOnly => currentStamina;

    public bool IsExhausted => isExhausted;
    public event Action<float, float> OnStaminaUpdate;
    public event Action OnExhausted;
    public event Action OnNotExhausted;

    // --------------------------------------------------
    // III. LIFE STATS & LOGIC
    // --------------------------------------------------
    [Header("Life Stats")]
    public int maxLives = 3;
    [SerializeField] private int currentLife;
    public int CurrentLife => currentLife;
    public event Action<int> OnLifeUpdate;
    public event Action OnGameOver;

    [Header("Respawn & Audio")]
    private Vector3 respawnPosition;
    private AudioSource audioSource;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            OnLocalPlayerStatsReady?.Invoke(this);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Initialize(maxSanity, maxStamina);

        if (Object.HasStateAuthority || Object.HasInputAuthority)
        {
            // Initial Respawn Point setup
            // Note: ต้องมี RespawnManager.Instance ในโปรเจกต์คุณ
            if (RespawnManager.Instance != null)
            {
                respawnPosition = RespawnManager.Instance.GetRespawnPosition(0);
            }
            else
            {
                respawnPosition = transform.position;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        timeSinceLastSanityChange += Runner.DeltaTime;
        RegenSanity();
    }


    public void Initialize(float s, float m)
    {
        if (Object.HasStateAuthority || Object.HasInputAuthority)
        {
            currentSanity = s;
        }
        currentStamina = m;
        currentLife = maxLives;
        isExhausted = false;

        OnSanityUpdate?.Invoke(currentSanity, maxSanity);
        OnStaminaUpdate?.Invoke(currentStamina, maxStamina);
        OnLifeUpdate?.Invoke(currentLife);

        regenTimer = regenCycleTime;
        timeSinceLastSanityChange = 0f;
    }

    private void RegenSanity()
    {
        if (!Object.HasInputAuthority) return;

        if (currentSanity >= maxSanity) return;
        if (timeSinceLastSanityChange < regenDelay) return;

        regenTimer -= Runner.DeltaTime;
        if (regenTimer <= 0f)
        {
            SetSanity(currentSanity + regenAmountPerCycle);
            regenTimer = regenCycleTime;
        }
    }

    private void SetSanity(float amount)
    {
        if (Object.HasStateAuthority || Object.HasInputAuthority)
        {
            CurrentSanity = amount;
        }
    }

    // ⭐⭐⭐ แก้ไข CS1061: เมธอดนี้ยังคงเป็น public เพื่อให้ SanityDrainOnLook.cs เรียกใช้ได้ ⭐⭐⭐
    public void ApplySanityDrain(float amount) { SetSanity(currentSanity - amount); }
    public void LoseSanity(float amount) { SetSanity(currentSanity - amount); }
    public void RecoverSanity(float amount) { SetSanity(currentSanity + amount); }

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

    public void RespawnPlayer()
    {
        SetSanity(maxSanity);
        currentStamina = maxStamina;
        isExhausted = false;
        transform.position = respawnPosition;
    }

    private void HandleDeath()
    {
        if (currentLife <= 0)
        {
            OnGameOver?.Invoke();
            return;
        }
        currentLife--;
        OnLifeUpdate?.Invoke(currentLife);
        RespawnPlayer();
    }
}