using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Called when the "Play" button is clicked
    public void PlayGame()
    {
        SceneManager.LoadScene("Map"); // make sure the scene name matches exactly
    }

    // Called when the "Quit" button is clicked
    public void QuitGame()
    {
        Debug.Log("Game Quit!"); // shows in console for testing in Editor
        Application.Quit(); // actually quits when built
    }
}
