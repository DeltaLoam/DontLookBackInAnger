using UnityEngine;



public class MapUI : MonoBehaviour

{

    [Header("UI References")]

    [Tooltip("Assign your Quit Canvas here")]

    public GameObject quitCanvas;



    private bool isPaused = false;



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



    void ToggleQuitMenu()

    {

        isPaused = !isPaused;



        if (quitCanvas != null)

            quitCanvas.SetActive(isPaused);



        // Toggle cursor and lock state

        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        Cursor.visible = isPaused;

    }



    public void QuitGame()

    {

        Debug.Log("Quit button clicked!");



#if UNITY_EDITOR

        // Stop play mode in Editor

        UnityEditor.EditorApplication.isPlaying = false;

#else

        // Quit application in build

        Application.Quit();

#endif

    }

}