using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI; // Required for Button

public class MenuNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // A singleton to make this script easily accessible from anywhere.
    public static MenuNetworkManager Instance { get; private set; }

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI Elements")]
    // Drag your Host and Client buttons here to disable them while connecting.
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;

    private NetworkRunner _runner;

    private void Awake()
    {
        // A robust singleton pattern that works with DontDestroyOnLoad.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            // If another instance already exists, destroy this new one.
            Destroy(this.gameObject);
        }
    }

    public void StartHost()
    {
        StartGame(GameMode.Host);
    }

    public void StartClient()
    {
        StartGame(GameMode.Client);
    }

    private async void StartGame(GameMode mode)
    {
        // Disable buttons to prevent the user from clicking again while connecting.
        if(_hostButton) _hostButton.interactable = false;
        if(_clientButton) _clientButton.interactable = false;

        // --- THIS IS THE KEY FIX ---
        // Ensure we always have a fresh NetworkRunner.
        // If one already exists from a previous session, get rid of it.
        if (_runner != null)
        {
            await _runner.Shutdown();
        }
        
        // Add a new NetworkRunner component to this GameObject.
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true; // Make sure this is true for player input.
        
        // Add this script to the runner's callbacks.
        _runner.AddCallbacks(this);

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        await _runner.StartGame(startGameArgs);
    }

    // This is called by Fusion when the runner is shut down for any reason.
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown. Reason: " + shutdownReason);
        // Destroy the persistent NetworkManager object.
        Destroy(this.gameObject);
        // Reload the menu scene to get back to a clean state.
        SceneManager.LoadScene("MenuScene");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            runner.LoadScene(gameSceneName);
        }
    }

    // --- You still need all the other empty callbacks for the interface ---
    #region Empty Callbacks
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}