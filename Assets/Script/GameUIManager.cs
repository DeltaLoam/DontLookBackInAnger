using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private Canvas inGameUICanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // THIS IS THE CRITICAL CHANGE
    private void Start()
    {
        // When the game scene starts, we must ensure two things:
        // 1. Our in-game UI is hidden.
        // 2. The cursor is UNLOCKED, so the Fusion menu works.
        // The HideInGameUI() function already does both of these things for us.
        HideInGameUI();
    }

    // This function is called by the player script when it spawns.
public void ShowInGameUI()
{
    if (inGameUICanvas != null) inGameUICanvas.enabled = true; // Use .enabled
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

public void HideInGameUI()
{
    if (inGameUICanvas != null) inGameUICanvas.enabled = false; // Use .enabled
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}
}