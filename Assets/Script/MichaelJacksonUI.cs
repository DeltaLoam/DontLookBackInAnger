using UnityEngine;

// This script should be attached to a GameObject within your "MichaelJackson" scene.
// Ensure you assign the Quit Canvas reference in the Inspector for this scene's instance.
public class MichaelJacksonUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign your Quit Canvas here (must be active in the 'MichaelJackson' scene)")]
    public GameObject quitCanvas;

    private bool isMenuOpen = false; // Renamed for clarity since the game isn't paused

    void Start()
    {
        // Hide quit menu at start
        if (quitCanvas != null)
            quitCanvas.SetActive(false);

        // Lock and hide cursor for FPS gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle quit menu when pressing ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleQuitMenu();
        }
    }

    /// <summary>
    /// Toggles the quit menu visibility and manages cursor locking/visibility.
    /// NOTE: This function does NOT pause the game (Time.timeScale is NOT affected).
    /// </summary>
    void ToggleQuitMenu()
    {
        isMenuOpen = !isMenuOpen; // Toggle the state

        if (quitCanvas != null)
            quitCanvas.SetActive(isMenuOpen);
        
        // Toggle cursor and lock state
        // When menu is open (isMenuOpen is true), the cursor is visible and unlocked.
        // When menu is closed (isMenuOpen is false), the cursor is hidden and locked.
        Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isMenuOpen;
    }

    /// <summary>
    /// Quits the application. This public function must be linked to the button's OnClick() event.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit button clicked! Application/Editor stopping...");

#if UNITY_EDITOR
        // Stop play mode in Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit application in build
        Application.Quit();
#endif
    }
}
