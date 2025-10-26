using UnityEngine;

// This script lives in the GameScene and controls the in-game UI.
public class GameUIManager : MonoBehaviour
{
    // A Singleton instance so the spawned player can easily find it.
    public static GameUIManager Instance { get; private set; }

    [Header("UI Panels")]
    // Drag the parent GameObject of your in-game UI (the Canvas or a panel) here.
    [SerializeField] private GameObject inGameUIPanel;

    private void Awake()
    {
        // Set up the Singleton pattern. When the GameScene loads, this will run.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // If for some reason another instance exists, destroy this one.
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // When the GameScene first loads, the in-game UI should be hidden
        // until the player has actually spawned.
        // We do this here just to be safe.
        if (inGameUIPanel != null)
        {
            inGameUIPanel.SetActive(false);
        }
    }

    // This is called by the PlayerNetworkController after it has spawned.
    public void ShowInGameUI()
    {
        if (inGameUIPanel != null)
        {
            inGameUIPanel.SetActive(true);
        }
    }
}