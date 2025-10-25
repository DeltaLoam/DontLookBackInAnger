using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections; // สำหรับ Coroutine
using Random = UnityEngine.Random; // ระบุชัดเจนว่าใช้ Random ของ Unity

public class SanityVisual : MonoBehaviour
{
    // --------------------------------------------------
    // I. SANITY DATA & THRESHOLDS
    // --------------------------------------------------
    [Header("Sanity Thresholds (จุดเริ่มต้นของระดับ)")]
    public float mildThreshold = 75f;
    public float mediumThreshold = 40f;
    public float severeThreshold = 15f;

    // โครงสร้างสำหรับตั้งค่า Visual Effects ใน Inspector
    [System.Serializable]
    public struct SanityLevelEffects
    {
        public float VignetteIntensity;
        public float ChromaticIntensity;
        public float GrainIntensity;
        public float Desaturation; // ใช้ค่า -100 ถึง 0
        public float ShakeFrequencySeconds;
        public float ShakeIntensityFactor;
    }

    [Header("Visual Effects by Level")]
    public SanityLevelEffects LevelHigh; // 100% - 75%
    public SanityLevelEffects LevelMild; // 75% - 40%
    public SanityLevelEffects LevelMedium; // 40% - 15%
    public SanityLevelEffects LevelSevere; // 15% - 0%

    // --------------------------------------------------
    // II. CAMERA SHAKE PHYSICS
    // --------------------------------------------------
    [Header("Camera Shake Physics")]
    public float maxBaseShakeIntensity = 0.05f;
    public float shakeDuration = 0.1f;

    // --------------------------------------------------
    // III. POST PROCESSING & CAMERA REFERENCES
    // --------------------------------------------------
    [Header("Post Processing & Camera")]
    public PostProcessVolume sanityVolume;
    public Camera mainCamera;

    // ตัวแปรส่วนตัวสำหรับอ้างอิง Post Processing Overrides
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private Grain grain; // ใช้ FilmGrain ตามที่คุณใช้ใน Volume
    private ColorGrading colorGrading; // ใช้ ColorGrading ตามที่คุณใช้ใน Volume

    private PlayerStats statsManager;
    private Coroutine shakeCoroutine;
    private Vector3 originalCameraLocalPosition;

    // --------------------------------------------------

    void Awake()
    {
        statsManager = GetComponent<PlayerStats>();
        if (statsManager == null)
        {
            Debug.LogError("PlayerStats component not found on this GameObject.");
            return;
        }

        // สมัครรับข้อมูลเมื่อ Sanity เปลี่ยนแปลง
        statsManager.OnSanityUpdate += UpdateVisualEffects;
    }

    void Start()
    {
        // ⭐ 1. ตรวจสอบการเชื่อมต่อ Volume
        if (sanityVolume == null || sanityVolume.profile == null)
        {
            Debug.LogError("Sanity Volume or Profile not assigned/found in SanityVisual. Please link the Post Process Volume.");
            return;
        }

        PostProcessProfile profile = sanityVolume.profile;

        Debug.Log("--- DEBUG: Starting Post Process Override Retrieval ---");

        // ⭐ 2. ดึง Overrides และ Debug การดึงค่า (สำคัญ)

        // Vignette
        if (profile.TryGetSettings(out vignette))
        {
            Debug.Log("✅ DEBUG SUCCESS: Vignette was successfully retrieved.");
        }
        else
        {
            Debug.LogError("❌ DEBUG FAILED: Vignette could not be retrieved. (Is it added and 'Intensity' controlled?)");
        }

        // Chromatic Aberration
        if (profile.TryGetSettings(out chromaticAberration))
        {
            Debug.Log("✅ DEBUG SUCCESS: Chromatic Aberration was successfully retrieved.");
        }
        else
        {
            Debug.LogError("❌ DEBUG FAILED: Chromatic Aberration could not be retrieved. (Is it added and 'Intensity' controlled?)");
        }

        // Film Grain
        if (profile.TryGetSettings(out grain))
        {
            Debug.Log("✅ DEBUG SUCCESS: FilmGrain was successfully retrieved.");
        }
        else
        {
            Debug.LogError("❌ DEBUG FAILED: FilmGrain could not be retrieved. (Is it added and 'Intensity' controlled?)");
        }

        // Color Grading
        if (profile.TryGetSettings(out colorGrading))
        {
            Debug.Log("✅ DEBUG SUCCESS: Color Grading was successfully retrieved.");
        }
        else
        {
            // ถ้า Color Grading ไม่เจอ ให้ลอง Color Adjustments (กรณีชื่อคลาสต่างกัน)
            // Note: ต้องเปลี่ยน private ColorGrading เป็น private ColorAdjustments ด้านบนด้วย
            // Debug.LogError("❌ DEBUG FAILED: Color Grading/Adjustments could not be retrieved. (Is it added and 'Saturation' controlled?)");
            Debug.LogError("❌ DEBUG FAILED: Color Grading could not be retrieved. (Is it added and 'Saturation' controlled?)");
        }

        Debug.Log("--- DEBUG: Post Process Override Retrieval Complete ---");

        if (mainCamera != null)
        {
            originalCameraLocalPosition = mainCamera.transform.localPosition;
        }

        // ตั้งค่าเริ่มต้นทันที (Sanity 100%)
        UpdateVisualEffects(statsManager.CurrentSanity, statsManager.maxSanity);
    }

    void OnDestroy()
    {
        // เลิกสมัครรับข้อมูลเพื่อป้องกัน Error
        if (statsManager != null)
        {
            statsManager.OnSanityUpdate -= UpdateVisualEffects;
        }
    }

    // --------------------------------------------------
    // CORE LOGIC: อัปเดต VISUAL EFFECTS ตาม SANITY LEVEL
    // --------------------------------------------------

    public void UpdateVisualEffects(float currentSanity, float maxSanity)
    {
        // ตรวจสอบว่า Overrides ถูกกำหนดค่าแล้ว
        if (vignette == null || chromaticAberration == null || grain == null || colorGrading == null)
        {
            // ลองเรียก Start() อีกครั้ง เผื่อ Start() แรกยังไม่เสร็จสมบูรณ์
            if (sanityVolume != null) Start();
            return;
        }

        // Debug Log เพื่อดูว่า Sanity Update ถูกเรียกจริงหรือไม่
        Debug.Log($"Sanity Update Received. Current Sanity: {currentSanity}. Applying effects.");

        SanityLevelEffects activeEffects;

        if (currentSanity >= mildThreshold)
        {
            activeEffects = LevelHigh;
        }
        else if (currentSanity >= mediumThreshold)
        {
            activeEffects = LevelMild;
        }
        else if (currentSanity >= severeThreshold)
        {
            activeEffects = LevelMedium;
        }
        else
        {
            activeEffects = LevelSevere;
        }

        // 1. อัปเดต Post Processing Overrides
        vignette.intensity.value = activeEffects.VignetteIntensity;
        chromaticAberration.intensity.value = activeEffects.ChromaticIntensity;
        grain.intensity.value = activeEffects.GrainIntensity;
        colorGrading.saturation.value = activeEffects.Desaturation;

        // Debug Log เพื่อดูค่าที่โค้ดพยายามตั้งค่า
        Debug.Log($"Vignette Set: {activeEffects.VignetteIntensity} | Desat Set: {activeEffects.Desaturation}");

        // 2. ควบคุม Camera Shake
        if (activeEffects.ShakeIntensityFactor > 0 && shakeCoroutine == null)
        {
            shakeCoroutine = StartCoroutine(ShakeCamera(activeEffects.ShakeFrequencySeconds, activeEffects.ShakeIntensityFactor));
        }
        else if (activeEffects.ShakeIntensityFactor == 0 && shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = originalCameraLocalPosition;
            }
        }
    }

    // --------------------------------------------------
    // COROUTINE: CAMERA SHAKE LOGIC
    // --------------------------------------------------

    private IEnumerator ShakeCamera(float frequency, float intensityFactor)
    {
        if (mainCamera == null) yield break;

        // ดำเนินการสั่นอย่างต่อเนื่อง
        while (true)
        {
            // คำนวณความรุนแรง
            float currentIntensity = maxBaseShakeIntensity * intensityFactor;

            // สร้างการสั่นแบบสุ่ม
            Vector3 randomOffset = new Vector3(
                Random.Range(-1f, 1f) * currentIntensity,
                Random.Range(-1f, 1f) * currentIntensity,
                Random.Range(-1f, 1f) * currentIntensity
            );

            // ใช้ Lerp เพื่อให้การสั่นดูนุ่มนวล
            float startTime = Time.time;
            Vector3 startPos = mainCamera.transform.localPosition;
            Vector3 targetPos = originalCameraLocalPosition + randomOffset;

            while (Time.time < startTime + shakeDuration)
            {
                mainCamera.transform.localPosition = Vector3.Lerp(startPos, targetPos, (Time.time - startTime) / shakeDuration);
                yield return null;
            }

            // ตั้งค่าตำแหน่งสุดท้าย
            mainCamera.transform.localPosition = targetPos;

            // หน่วงเวลาตาม Frequency (ความถี่ในการสั่น)
            yield return new WaitForSeconds(frequency);
        }
    }
}