using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called by the Play button
    public void PlayGame()
    {
        // Replace "GameScene" with your actual game scene name
        SceneManager.LoadScene("Map");
    }

    // Called by the Quit button
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();

        // This line helps when testing inside the editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}