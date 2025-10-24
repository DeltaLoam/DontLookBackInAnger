using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;

    private void Start()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void OnStartButton()
    {
        // Load your multiplayer lobby scene
        SceneManager.LoadScene("MultiplayerLobby");
    }

    public void OnQuitButton()
    {
        Application.Quit();

        // Just for debugging in Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
